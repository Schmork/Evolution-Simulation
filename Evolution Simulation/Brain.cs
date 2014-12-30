using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    class Brain
    {
        public Neuron[][] Neurons;

        public class Neuron
        {
            public double[] Connections;
            public double Bias;
            public double Value;

            public Neuron() { }
            public Neuron(double bias, double[] connections)
            {
                Connections = connections;
                Bias = bias;
            }
        }


        public static double MaxOffset = 5;
        public static double MaxFactor = 5;
        public static double MaxWeight = 5;
        public static double MutationChance, MutationChange;
        /// <summary>
        /// Number of layers *in addition* to the input layer. Layers = 2 results in 3 layers: input, web and output. Web + output = 2
        /// </summary>
        public static int LayerCount = 3;
        /// <summary>
        /// Number of 0-level neurons, which do not get input from other neurons.
        /// </summary>
        public static int InputNeuronsCount = World.Horizon * 12 + 2;    // horizon * 3 directions * 4 cell types + energy + age
        /// <summary>
        /// Number of neurons a web layer has.
        /// </summary>
        public static int NeuronsPerLayerCount = 20;
        /// <summary>
        /// Number of output neurons.
        /// </summary>
        public static int OutputNeuronsCount = typeof(ActionVector).GetFields().Count();
        public static int ChanceDivisor = 100000;
        public static double ChangeFactor = 0.001;

        private Random _rnd;

        public Brain(Random rnd, Brain brain)
        {
            Neurons = new Neuron[LayerCount + 1][];
            _rnd = rnd;
            createNetwork(brain);
        }

        private void createNetwork(Brain brain)
        {
            // first layer
            createInputLayer();

            // mid layers
            for (int layer = 1; layer < LayerCount; layer++)
            {
                if (brain == null)
                {
                    createWebLayer(layer, NeuronsPerLayerCount);
                }
                else
                {
                    mutateLayer(layer, brain);
                }
            }

            // last layer
            if (brain == null)
            {
                createWebLayer(LayerCount, OutputNeuronsCount);
            }
            else
            {
                mutateLayer(LayerCount, brain);
            }
        }

        private void createInputLayer()
        {
            Neurons[0] = new Neuron[InputNeuronsCount];

            for (int i = 0; i < InputNeuronsCount; i++)
            {
                Neurons[0][i] = new Neuron();
            }
        }

        private void createWebLayer(int layer, int neuronCount)
        {
            Neurons[layer] = new Neuron[neuronCount];
            //var prevLayer = NeuronsOld[layer - 1];

            var prevLayer = Neurons[layer - 1];
            for (int i = 0; i < neuronCount; i++)
            {
                //NeuronsOld[layer][i] = createWebbedNeuron(prevLayer);
                Neurons[layer][i] = createWebbedNeuron(prevLayer);
            }
        }

        private Neuron createWebbedNeuron(Neuron[] prevLayer)
        {
            var connections = new double[prevLayer.Length];
            double bias;

            var chance = 1;         // to create a new, random network, enforce mutation (else everything would remain 0)

            for (int n = 0; n < prevLayer.Length; n++)
            {
                connections[n] = mutate(chance, MaxWeight);
            }
            bias = mutate(chance, MaxOffset);

            return new Neuron(bias, connections);
        }

        private void mutateLayer(int layer, Brain brain)
        {
            var length = brain.Neurons[layer].Length;
            Neurons[layer] = new Neuron[length];
            for (int i = 0; i < length; i++)
            {
                Neurons[layer][i] = mutateNeuron(brain.Neurons[layer][i], Neurons[layer - 1]);
            }
        }

        private Neuron mutateNeuron(Neuron webNeuron, Neuron[] prevLayer)
        {
            var length = webNeuron.Connections.Length;
            var connections = new double[length];

            var chance = MutationChance / (double)ChanceDivisor;         // to create a new, random network, enforce mutation (else everything would remain 0)
            var change = MutationChange * ChangeFactor;

            for (int n = 0; n < length; n++)
            {
                connections[n] = Transform.StayInBounds(
                            webNeuron.Connections[n]
                            + mutate(chance, MaxWeight), MaxWeight);
            }
            var bias = Transform.StayInBounds(
                        webNeuron.Bias + mutate(chance, change), MaxOffset);

            return new Neuron(bias, connections);
        }

        private double mutate(double chance, double change)
        {
            double x = 0;
            if (chance >= _rnd.NextDouble())
            {
                x += _rnd.NextDouble() * 2 * change - change;
            }
            return x;
        }

        /// <summary>
        /// Updates values of InputNeurons and resets all other neurons.
        /// </summary>
        public void UpdateInputs(InputVector inputVector)
        {
            for (int i = 0; i < inputVector.InputsForBrain.Length; i++)
            {
                Neurons[0][i].Value = inputVector.InputsForBrain[i];
            }
        }
        
        /// <summary>
        /// Calculates all required neuron values. Returns values of output layer
        /// </summary>
        public double[] GetOutput()
        {
            var nLength = Neurons.Length;

            #region calculate values for all neurons
            for (int i = 1; i < nLength; i++)   // cycle all neuron layers after the input layer
            {
                for (int j = 0; j < Neurons[i].Length; j++)     // cycle all neurons in this layer
                {
                    var value = 0.0;
                    for (int k = 0; k < Neurons[i - 1].Length; k++)     // previous layer's neurons - get connectionStrengths towards them
                    {
                        value += Neurons[i - 1][k].Value * Neurons[i][j].Connections[k];    // get cumulative value of all previous layer's neurons * connectionStrength
                    }
                    Neurons[i][j].Value = Transform.StayInBounds(value, 1);         // this will be the value of the current neuron
                }
            }
            #endregion

            #region get output from last layer
            var outputLayer = Neurons[nLength - 1];
            var outputArray = new double[outputLayer.Length];
            for (int i = 0; i < outputLayer.Length; i++)
			{
                outputArray[i] = outputLayer[i].Value;
            }
            return softMax(outputArray);
            #endregion
        }

        private double[] softMax(double[] outputs)
        {
            var length = outputs.Length;
            var denom = 0.0;
            for (int i = 0; i < length; i++)
                denom += Math.Exp(outputs[i]);

            var results = new double[length];
            for (int i = 0; i < length; i++)
                results[i] = Math.Exp(outputs[i]) / denom;
            return results;
        }

        public double GeneticDistance(Brain brain)
        {
            double dist = 0;
            for (int layer = 1; layer < Neurons.Length; layer++)        // skip layer 0 (input layer - they don't have any inherent values which could be different)
            {
                for (int neuron = 0; neuron < Neurons[layer].Length; neuron++)
                {
                    var a = Neurons[layer][neuron];
                    var b = brain.Neurons[layer][neuron];

                    dist += Math.Abs(a.Bias - b.Bias);

                    for (int con = 0; con < a.Connections.Length; con++)
                    {
                        //dist += Math.Abs(a.Connections[con].Weight - b.Connections[con].Weight);
                    }
                }
            }
            return dist;
        }
    }
}

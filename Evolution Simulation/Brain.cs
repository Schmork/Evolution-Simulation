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

        public static double MaxOffset = 5;
        public static double MaxFactor = 5;
        public static double MaxWeight = 5;
        public static double MutationChance, MutationChange;
        public static int Layers = 3;
        public static int InputNeurons = World.Horizon * 12 + 2;    // horizon * 3 directions * 4 cell types + energy + age
        private static int _neuronsPerLayer = 20;
        private static int _outputNeuronsCount = typeof(ActionVector).GetFields().Count();
        public static int ChanceDivisor = 100000;
        public static double ChangeFactor = 0.001;

        private Random _rnd;

        public Brain(Random rnd, Brain brain)
        {
            Neurons = new Neuron[Layers + 1][];
            _rnd = rnd;
            createNetwork(brain);
        }
        
        private void createNetwork(Brain brain)
        {
            createInputLayer();
            for (int layer = 1; layer < Layers; layer++)
            {
                if (brain == null)
                {
                    createWebLayer(layer, _neuronsPerLayer);
                }
                else
                {
                    mutateLayer(layer, brain);
                }
            }

            if (brain == null)
            {
                createWebLayer(Layers, _outputNeuronsCount);
            }
            else
            {
                mutateLayer(Layers, brain);
            }
        }

        private void createInputLayer()
        {
            Neurons[0] = new Neuron[InputNeurons];
            for (int i = 0; i < InputNeurons; i++)
            {
                Neurons[0][i] = new Neuron();
            }
        }

        private void createWebLayer(int layer, int neuronCount)
        {
            Neurons[layer] = new Neuron[neuronCount];
            var prevLayer = Neurons[layer - 1];

            for (int i = 0; i < neuronCount; i++)
            {
                Neurons[layer][i] = createWebbedNeuron(prevLayer);
            }
        }
        
        private Neuron createWebbedNeuron(Neuron[] prevLayer)
        {
            var connections = new Neuron.Connection[prevLayer.Length];
            double bias;

            var chance = 1;         // to create a new, random network, enforce mutation (else everything would remain 0)

            for (int n = 0; n < prevLayer.Length; n++)
            {
                var weight = mutate(chance, MaxWeight);
                var sourceNeuron = prevLayer[n];
                connections[n] = new Neuron.Connection(sourceNeuron, weight);
            }
            bias = mutate(chance, MaxOffset);

            return new Neuron(connections, bias);
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
            var connections = new Neuron.Connection[length];

            var chance = MutationChance / (double)ChanceDivisor;         // to create a new, random network, enforce mutation (else everything would remain 0)
            var change = MutationChange * ChangeFactor;

            for (int n = 0; n < length; n++)
            {
                var weight = Normalizer.StayInBounds(
                            webNeuron.Connections[n].Weight
                            + mutate(chance, MaxWeight), MaxWeight);
                connections[n] = new Neuron.Connection(prevLayer[n], weight);
            }
            var bias = Normalizer.StayInBounds(
                        webNeuron.Bias + mutate(chance, change), MaxOffset);

            return new Neuron(connections, bias);
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

        public void UpdateInputs(InputVector inputVector)
        {
            for (int i = 0; i < inputVector.InputsForBrain.Length; i++)
            {
                Neurons[0][i].SetValue(inputVector.InputsForBrain[i]);
            }
        }

        public double[] GetOutput()
        {
            var outputNeurons = Neurons[Neurons.Length - 1];
            var outputs = new double[_outputNeuronsCount];

            for (int i = 0; i < outputs.Count(); i++)
            {
                outputs[i] = outputNeurons[i].GetValue();
            }
            return softMax(outputs);
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
                        dist += Math.Abs(a.Connections[con].Weight - b.Connections[con].Weight);
                    }
                }
            }
            var z = 5;
            return dist;
        }
    }
}

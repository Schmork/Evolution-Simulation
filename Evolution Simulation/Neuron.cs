﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    class Neuron
    {
        public class Connection
        {
            public double Weight;
            public Neuron Source;

            public Connection(Neuron source, double weight)
            {
                Weight = weight;
                Source = source;
            }
        }

        public Connection[] Connections;
        public double Bias;
        
        private double _value;
        public bool IsInputNeuron;
        
        public Neuron (Connection[] connections, double bias)
        {
            Connections = connections;
            Bias = bias;
        }

        public Neuron()
        {
            IsInputNeuron = true;
        }

        public void SetValue(double x)
        {
            if (!IsInputNeuron) throw new InvalidOperationException();
            _value = Normalizer.StayInBounds(x, 5);
        }

        public double GetValue()
        {
            if (IsInputNeuron)
            {
                return _value;
            }
            else
            {
                double sum = Bias;
                foreach (var con in Connections)
                {
                    sum += con.Source.GetValue() * con.Weight;
                }
                return Normalizer.StayInBounds(sum, 1);
            }
        }
    }
}
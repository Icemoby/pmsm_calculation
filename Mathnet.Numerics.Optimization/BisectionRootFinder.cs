﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public class BisectionRootFinder
    {

        public double ObjectiveTolerance { get; set; }
        public double XTolerance { get; set; }
        public double LowerExpansionFactor { get; set; }
        public double UpperExpansionFactor { get; set; }
        public int MaxExpansionSteps { get; set; }
        public bool AllowInfiniteObjectiveValues { get; set; }
        public int MaxBisectionSteps { get; set; }

        public BisectionRootFinder(double objective_tolerance=1e-5, double x_tolerance=1e-5, double lower_expansion_factor=-1.0, double upper_expansion_factor=-1.0, int max_expansion_steps=10, bool allow_infinite_objective_values=false, int max_bisection_steps=1000)
        {
            this.ObjectiveTolerance = objective_tolerance;
            this.XTolerance = x_tolerance;
            this.LowerExpansionFactor = lower_expansion_factor;
            this.UpperExpansionFactor = upper_expansion_factor;            
            this.MaxExpansionSteps = max_expansion_steps;
            this.AllowInfiniteObjectiveValues = allow_infinite_objective_values;
            this.MaxBisectionSteps = max_bisection_steps;
        } 

        public double FindRoot(Func<double, double> objective_function, double lower_bound, double upper_bound)
        {
            double lower_val = objective_function(lower_bound);
            double upper_val = objective_function(upper_bound);

            if (lower_val == 0.0)
                return lower_bound;
            if (upper_val == 0.0)
                return upper_bound;

            this.ValidateEvaluation(lower_val, lower_bound);
            this.ValidateEvaluation(upper_val, upper_bound);

            if (Math.Sign(lower_val) == Math.Sign(upper_val) && this.LowerExpansionFactor <= 1.0 && this.UpperExpansionFactor <= 1.0)
                throw new Exception("Bounds do not necessarily span a root, and StepExpansionFactor is not set to expand the interval in this case.");

            int expansion_steps = 0;
            while (Math.Sign(lower_val) == Math.Sign(upper_val) && expansion_steps < this.MaxExpansionSteps)
            {
                double midpoint = 0.5 * (upper_bound + lower_bound);
                double range = upper_bound - lower_bound;
                if (this.UpperExpansionFactor <= 0.0 || (this.LowerExpansionFactor > 0.0 && Math.Abs(lower_val) < Math.Abs(upper_val)) )
                {
                    lower_bound = upper_bound - this.LowerExpansionFactor * range;
                    lower_val = objective_function(lower_bound);
                    this.ValidateEvaluation(lower_val, lower_bound);
                }
                else
                {
                    upper_bound = lower_bound + this.UpperExpansionFactor * range;
                    upper_val = objective_function(upper_bound);
                    this.ValidateEvaluation(upper_val, upper_bound);
                }
                expansion_steps += 1;
            }

            if (Math.Sign(lower_val) == Math.Sign(upper_val) && expansion_steps == this.MaxExpansionSteps)
                throw new MaximumIterationsException("Could not bound root in maximum expansion iterations.");

            int bisection_steps = 0;
            while ( (Math.Abs(upper_val - lower_val) > 0.5 * this.ObjectiveTolerance || Math.Abs(upper_bound - lower_bound) > 0.5 * this.XTolerance) && bisection_steps < this.MaxBisectionSteps)
            {
                double midpoint = 0.5 * (upper_bound + lower_bound);
                double midval = objective_function(midpoint);
                this.ValidateEvaluation(midval, midpoint);

                if (Math.Sign(midval) == Math.Sign(lower_val))
                {
                    lower_bound = midpoint;
                    lower_val = midval;
                }
                else if (Math.Sign(midval) == Math.Sign(upper_val))
                {
                    upper_bound = midpoint;
                    upper_val = midval;
                }
                else
                {
                    return midpoint;
                }

                bisection_steps += 1;
            }

            if (Math.Abs(upper_val - lower_val) <= 0.5 * this.ObjectiveTolerance && Math.Abs(upper_bound - lower_bound) <= 0.5 * this.XTolerance)
                return 0.5 * (lower_bound + upper_bound);
            else
                throw new MaximumIterationsException("Bisection did not find root in interval; function is probably non-monotone or discontinuous.");
        }

        private void ValidateEvaluation(double output, double input)
        {
            if (Double.IsNaN(output) || (!this.AllowInfiniteObjectiveValues && Double.IsInfinity(output)))
                throw new Exception(String.Format("Objective function returned non-finite result: f({0}) = {1}", input, output));
        }
    }
}

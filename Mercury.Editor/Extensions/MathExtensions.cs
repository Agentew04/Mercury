using System;

namespace Mercury.Editor.Extensions;

public static class MathExtensions {
    extension(Math) {
        public static bool Approximately(double a, double b) {
            const double epsilon = 0.1;
            double diff = a - b;
            if (diff < 0) diff = -diff;
            return diff < epsilon;
        }
    }
    
    extension(MathF) {
        public static bool Approximately(float a, float b) {
            const double epsilon = 0.1;
            double diff = a - b;
            if (diff < 0) diff = -diff;
            return diff < epsilon;
        }
    }
}
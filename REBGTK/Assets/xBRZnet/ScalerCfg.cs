namespace xBRZNet
{
    [System.Serializable]
    public class ScalerCfg
    {
        public ScalerCfg()
        {

        }

        public ScalerCfg(float DominantDirectionThreshold, float SteepDirectionThreshold)
        {
            this.DominantDirectionThreshold = (double)DominantDirectionThreshold;
            this.SteepDirectionThreshold = (double)SteepDirectionThreshold;
        }

        // These are the default values:
        public double LuminanceWeight { get; set; } = 1;
        public double EqualColorTolerance { get; set; } = 30;
        public double DominantDirectionThreshold { get; set; } = 3.6;
        public double SteepDirectionThreshold { get; set; } = 2.2;
    }
}

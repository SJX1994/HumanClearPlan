using UnityEngine;
namespace JellyData
{
    public class JellyDataSet
    {
      [SerializeField]
      private static Shader shader;
      [SerializeField]
      private static Color color;
      [SerializeField]
      public const float Spring = 15f;
      [SerializeField]
      public const float Damping = 0.2f;
      [SerializeField]
      public const float Namida = 5;
      [SerializeField]
      public const int CountMax = 4 ;
      [SerializeField]
      public static Type type = Type.Explore;
        public enum Type
        {
            Wave,
            Explore,
        }
    
    }
}


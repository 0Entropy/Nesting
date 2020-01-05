using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Nesting
{
    using UnityEngine;

    public class Config
    {
        public static float initAngle = 0;
        public static int angleNum = 1;

        public static float spacing = 3f;// * PDF.PDFPreferences.DPMM;//mm

        public static float leftMargin = 6f;// * PDF.PDFPreferences.DPMM;
        public static float rightMatgin = 6f;// * PDF.PDFPreferences.DPMM;

        public static float lowerCutMargin = 50f;//刀口位置
        public static float upperCutMargin = 10f;//
    }


    //[Serializable]
    public class Paper
    {
        //
        //正度纸张： 大度纸张  开数/尺寸 单位（mm） 单位（mm） 
        //1开 2开 3开 4开 6开 8开 16开 32开 64开
        //787×1092 540×780 360×780 390×543 360×390 270×390 195×270 195×135 135×95  
        //889×1194 590×880 395×880 440×590 395×440 295×440 220×295 220×145 110×145 

        public enum Size
        {
            Customize,
            //大度
            D_A0,//841×1189
            D_A1,//594×841
            D_A2,//420×594
            D_A3,//297×420
            D_A4,//210×297
            //正度
        }

        /// <summary>
        /// 克重
        /// </summary>
        public enum Gram
        {
            C250,
            C300
        }

        public static Dimension LookupDimension(Size size )
        {
            return Configuration.Instance.Lookup ( size );
        }

        [Serializable]
        public class Dimension
        {
            public Size size;
            public string chineseName;
            public float width;
            public float height;

            /// <summary>
            /// Factory Model
            /// </summary>
            /// <param name="size"></param>
            /// <returns></returns>
            public static Dimension Create ( Size size )
            {
                switch ( size )
                {
                    case Size.D_A0:
                        return new Dimension ( ) { size = size, chineseName = "大度全开", width = 1189, height = 841 };
                    case Size.D_A1:
                        return new Dimension ( ) { size = size, chineseName = "大度对开", width = 841, height = 594 };
                    case Size.D_A2:
                        return new Dimension ( ) { size = size, chineseName = "大度四开", width = 594, height = 420 };
                    case Size.D_A3:
                        return new Dimension ( ) { size = size, chineseName = "大度八开", width = 420, height = 297 };
                    case Size.D_A4:
                        return new Dimension ( ) { size = size, chineseName = "大度十六开", width = 297, height = 210 };
                    default://默认返回大度1开；
                        return new Dimension ( ) { size = size, chineseName = "大度1开", width = 1189, height = 841 };
                }
            }
            
        }
        
        class Configuration
        {
            [SerializeField]
            List<Dimension> dimensionLookup;

            private Configuration ( )
            {
                DuildDimens ( );
            }

            private static Configuration instance;
            internal static Configuration Instance
            {
                get
                {
                    if ( instance == null )
                        instance = new Configuration ( );
                    return instance;
                }
            }

            internal Dimension Lookup(Size size )
            {
                return dimensionLookup.Find ( d => d.size == size );
            }

            internal Dimension Lookup(string name )
            {
                return dimensionLookup.Find ( d => d.chineseName == name );
            }

            internal void DuildDimens ( ){
                dimensionLookup = new List<Dimension> ( );
                dimensionLookup.Add ( Dimension.Create ( Size.D_A0 ) );
                dimensionLookup.Add ( Dimension.Create ( Size.D_A1 ) );
            }

            internal void Add ( Dimension customize )
            {
                dimensionLookup.Add ( customize );
            }

            internal void SaveTo ( string path )
            {
                var jsonArgs = JsonFx.Json.JsonWriter.Serialize(dimensionLookup);
                Console.WriteLine ( jsonArgs );
            }

        }
        
    }
}

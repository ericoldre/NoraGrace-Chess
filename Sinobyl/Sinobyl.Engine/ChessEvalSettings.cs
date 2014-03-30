using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Sinobyl.Engine
{

    public class ChessEvalSettings
    {

        #region helperclasses
        public class ChessEvalSettingsMobility
        {
            public int ExpectedAttacksAvailable { get; set; }
            public int AmountPerAttackDefault { get; set; }
        }

        public class ChessEvalSettingsWeight
        {
            public ChessGameStageDictionary<int> Material = new ChessGameStageDictionary<int>();
            public ChessGameStageDictionary<int> PcSq = new ChessGameStageDictionary<int>();
            public ChessGameStageDictionary<int> Mobility = new ChessGameStageDictionary<int>();
        }
            

        #endregion

        #region DefaultEvalSettings

        public static ChessEvalSettings Default()
        {
            ChessEvalSettings retval = new ChessEvalSettings()
            {
                Weight = new ChessEvalSettingsWeight()
                {
                    Material = new ChessGameStageDictionary<int>() { Opening = 100, Endgame = 100 },
                    PcSq = new ChessGameStageDictionary<int>() { Opening = 100, Endgame = 100 },
                    Mobility = new ChessGameStageDictionary<int>() { Opening = 100, Endgame = 100 },
                },
                MaterialValues = new ChessPieceTypeDictionary<ChessGameStageDictionary<int>>()
                {
                    Pawn = new ChessGameStageDictionary<int>() { Opening = 100, Endgame = 100 },
                    Knight = new ChessGameStageDictionary<int>() { Opening = 300, Endgame = 300 },
                    Bishop = new ChessGameStageDictionary<int>() { Opening = 300, Endgame = 300 },
                    Rook = new ChessGameStageDictionary<int>() { Opening = 500, Endgame = 500 },
                    Queen = new ChessGameStageDictionary<int>() { Opening = 900, Endgame = 900 },
                    King = new ChessGameStageDictionary<int>() { Opening = 0, Endgame = 0 },
                },

                MaterialBishopPair = new ChessGameStageDictionary<int>() { Opening = 30, Endgame = 50 },

                PcSqTables = new ChessPieceTypeDictionary<ChessGameStageDictionary<ChessPositionDictionary<int>>>()
                {
                    Pawn = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            0,  0,  0,  0,  0,  0,  0,  0,	
			                -10, -5, 0, 5,  5,  0, -5, -10,
			                -10, -5, 0, 5,  5,  0, -5, -10,	
			                -10, -5, 0, 10, 10, 0, -5, -10,	
			                -10, -5, 5, 15, 15, 0, -5, -10,	
			                -10, -5, 5, 10, 10, 0, -5, -10,	
			                -10, -5, 0, 5,  5,  5, -5, -10,	
			                 0,  0,  0,  0,  0,  0,  0,  0,	
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            0,  0,  0,  0,  0,  0,  0,  0,	
			                20, 25, 34, 38, 38, 34, 25, 20,
			                12, 20, 24, 32, 32, 24, 20, 12,	
			                 6, 11, 18,	27, 27, 16,	11,  6,	
			                 4,  7, 10, 12, 12, 10,  7,  4,	
			                -3, -3, -3, -3, -3, -3, -3,  -3,	
			                -10,-10,-10,-10,-10,-10,-10,-10,	
			                 0,  0,  0,  0,  0,  0,  0,  0,	
                        })
                    },
                    Knight = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
			                 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			                 3,	 7,	14,	16,	16,	14,	 7,	 3,	
			                 4,	 9,	15,	16,	16,	15,	 9,	 4,	
			                 4,	 9,	13,	15,	15,	13,	 9,	 4,	
			                 3,	 7,	11,	13,	13,	11,	 7,	 3,	
			                 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			                -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
			                 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			                 3,	 7,	14,	16,	16,	14,	 7,	 3,	
			                 4,	 9,	15,	16,	16,	15,	 9,	 4,	
			                 4,	 9,	13,	15,	15,	13,	 9,	 4,	
			                 3,	 7,	11,	13,	13,	11,	 7,	 3,	
			                 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			                -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
                        })
                    },
                    Bishop = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
			                -3,	 9,	11,	11,	11,	11,	 9,	-3,	
			                -3,	 8,	12,	12,	12,	12,	 8,	-3,	
			                -3,	 0,	12,	12,	12,	12,	 6,	-3,	
			                -3,	 0,	12,	12,	12,	12,	 6,	-3,	
			                 6,	 8,	12,	12,	12,	12,	 8,	 6,	
			                 6,	 9,	11,	11,	11,	11,	 9,	 6,	
			                -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,	
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
			                -3,	 9,	11,	11,	11,	11,	 9,	-3,	
			                -3,	 8,	12,	12,	12,	12,	 8,	-3,	
			                -3,	 0,	12,	12,	12,	12,	 6,	-3,	
			                -3,	 0,	12,	12,	12,	12,	 6,	-3,	
			                 6,	 8,	12,	12,	12,	12,	 8,	 6,	
			                 6,	 9,	11,	11,	11,	11,	 9,	 6,	
			                -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,		
                        })
                    },
                    Rook = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            0,	0,	4,	6,	6,	4,	0,	0,	
			                10,	10,	14,	16,	16,	14,	10,	10,
			                0,	0,	4,	6,	6,	4,	0,	0,
			                0,	0,	4,	6,	6,	4,	0,	0,
			                0,	0,	4,	6,	6,	4,	0,	0,	
			                0,	0,	4,	6,	6,	4,	0,	0,	
			                0,	0,	4,	6,	6,	4,	0,	0,	
			                0,	0,	4,	6,	6,	4,	0,	0	
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0	
                        })
                    },
                    Queen = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                      		0,	0,	0,	0,	0,	0,	0,	0,	
			                0,  0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	2,	2,	0,	0,	0,	
			                0,	0,	0,	2,	2,	0,	0,	0,	
			                0,	1,	1,	1,	1,	0,	0,	0,	
			                0,	0,	1,	1,	1,	0,	0,	0,	
			                -2,-2, -2,  0,	0, -2, -2, -2
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                            0,	0,	0,	0,	0,	0,	0,	0,	
			                0,  0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	0,	0,	0,	0,	0,	
			                0,	0,	0,	2,	2,	0,	0,	0,	
			                0,	0,	0,	2,	2,	0,	0,	0,	
			                0,	1,	1,	1,	1,	0,	0,	0,	
			                0,	0,	1,	1,	1,	0,	0,	0,	
			                -2,-2, -2,  0,	0, -2, -2, -2	
                        })
                    },
                    King = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                    {
                        Opening = new ChessPositionDictionary<int>(new int[]
                        {
                            -80,-80,-80,-80,-80,-80,-80,-80,
			                -80,-80,-80,-80,-80,-80,-80,-80,
			                -80,-80,-80,-80,-80,-80,-80,-80,
			                -60,-60,-60,-60,-60,-60,-60,-60,
			                -40,-40,-40,-40,-40,-40,-40,-40,
			                -7,	-15,-15,-15,-15,-15,-15,-7,	
			                -5,	-5,	-12,-12,-12,-12,-5,	-5,	
			                 3,	 3,	 8,	-5,  -8,-5, 10,	 5		
                        }),
                        Endgame = new ChessPositionDictionary<int>(new int[]
                        {
                             -8, -8, -8, -8, -8, -8, -8, -8,
			                 -8, -0, -0, -0, -0, -0, -0, -8,
			                 -8, -0, 05, 05, 05, 05, -0, -8,
			                 -8, -0, 05, 30, 30, 05, -0, -8,
			                 -8, -0, 05, 30, 30, 05, -0, -8,
			                 -8, -0, 05, 30, 30, 05, -0, -8,
			                 -8, -0, -0, -0, -0, -0, -0, -8,
			                 -8, -8, -8, -8, -8, -8, -8, -8,
                        })
                    },
                },



                PawnPassedValues = new ChessGameStageDictionary<ChessPositionDictionary<int>>()
                {
                    Opening = new ChessPositionDictionary<int>(new int[]
                     {
                          0,  0,  0,  0,  0,  0,  0,  0,
			             50, 50, 50, 50, 50, 50, 50, 50,
			             30, 30, 30, 30, 30, 30, 30, 30,
			             20, 20, 20, 20, 20, 20, 20, 20,
			             15, 15, 15, 15, 15, 15, 15, 15,
			             15, 15, 15, 15, 15, 15, 15, 15,
			             15, 15, 15, 15, 15, 15, 15, 15,
			              0,  0,  0,  0,  0,  0,  0,  0
                     }),
                    Endgame = new ChessPositionDictionary<int>(new int[]
                     {
                      	  0,  0,  0,  0,  0,  0,  0,  0,
			            140,140,140,140,140,140,140,140,
			             90, 90, 90, 90, 90, 90, 90, 90,
			             60, 60, 60, 60, 60, 60, 60, 60,
			             40, 40, 40, 40, 40, 40, 40, 40,
			             25, 25, 25, 25, 25, 25, 25, 25,
			             15, 15, 15, 15, 15, 15, 15, 15,
			              0,  0,  0,  0,  0,  0,  0,  0	
                     })
                },
                PawnDoubled = new ChessGameStageDictionary<int>() { Opening = 10, Endgame = 25 },
                PawnIsolated = new ChessGameStageDictionary<int>() { Opening = 15, Endgame = 25 },
                PawnUnconnected = new ChessGameStageDictionary<int>(){ Opening = 5, Endgame = 10},
                
                PawnPassed8thRankScore = 332,
                PawnPassedRankReduction = .60f,
                PawnPassedDangerPct = .10f,
                PawnPassedMinScore = 10,
                PawnPassedOpeningPct = .31f,

                Mobility = new ChessPieceTypeDictionary<ChessGameStageDictionary<ChessEvalSettingsMobility>>()
                {
                    Knight = new ChessGameStageDictionary<ChessEvalSettingsMobility>()
                    {
                        Opening = new ChessEvalSettingsMobility() { ExpectedAttacksAvailable = 4, AmountPerAttackDefault = 4 },
                        Endgame = new ChessEvalSettingsMobility() { ExpectedAttacksAvailable = 4, AmountPerAttackDefault = 4 },
                    },
                    Bishop = new ChessGameStageDictionary<ChessEvalSettingsMobility>()
                    {
                        Opening = new ChessEvalSettingsMobility() { ExpectedAttacksAvailable = 6, AmountPerAttackDefault = 5 },
                        Endgame = new ChessEvalSettingsMobility() { ExpectedAttacksAvailable = 6, AmountPerAttackDefault = 5 },
                    },
                    Rook = new ChessGameStageDictionary<ChessEvalSettingsMobility>()
                    {
                        Opening = new ChessEvalSettingsMobility() { ExpectedAttacksAvailable = 5, AmountPerAttackDefault = 2 },
                        Endgame = new ChessEvalSettingsMobility() { ExpectedAttacksAvailable = 7, AmountPerAttackDefault = 4 },
                    },
                    Queen = new ChessGameStageDictionary<ChessEvalSettingsMobility>()
                    {
                        Opening = new ChessEvalSettingsMobility() { ExpectedAttacksAvailable = 9, AmountPerAttackDefault = 1 },
                        Endgame = new ChessEvalSettingsMobility() { ExpectedAttacksAvailable = 13, AmountPerAttackDefault = 2 },
                    },
                }
            };

            
            return retval;
        }

        #endregion


        public ChessPieceTypeDictionary<ChessGameStageDictionary<ChessPositionDictionary<int>>> PcSqTables = new ChessPieceTypeDictionary<ChessGameStageDictionary<ChessPositionDictionary<int>>>();
        public ChessPieceTypeDictionary<ChessGameStageDictionary<int>> MaterialValues = new ChessPieceTypeDictionary<ChessGameStageDictionary<int>>();
        public ChessGameStageDictionary<int> MaterialBishopPair = new ChessGameStageDictionary<int>();

        public ChessGameStageDictionary<ChessPositionDictionary<int>> PawnPassedValues = new ChessGameStageDictionary<ChessPositionDictionary<int>>();
        public ChessGameStageDictionary<int> PawnDoubled = new ChessGameStageDictionary<int>();
        public ChessGameStageDictionary<int> PawnIsolated = new ChessGameStageDictionary<int>();
        public ChessGameStageDictionary<int> PawnUnconnected = new ChessGameStageDictionary<int>();
        public ChessPieceTypeDictionary<ChessGameStageDictionary<ChessEvalSettingsMobility>> Mobility = new ChessPieceTypeDictionary<ChessGameStageDictionary<ChessEvalSettingsMobility>>();
        public ChessEvalSettingsWeight Weight = new ChessEvalSettingsWeight();
        
        public int PawnPassed8thRankScore = 0;
        public double PawnPassedRankReduction = 0;
        public int PawnPassedMinScore = 0;
        public double PawnPassedDangerPct = 0;
        public double PawnPassedOpeningPct = 0;

        public ChessEvalSettings CloneDeep()
        {
            return DeserializeObject<ChessEvalSettings>(SerializeObject<ChessEvalSettings>(this));
        }

        public override bool Equals(object obj)
        {
            ChessEvalSettings other = obj as ChessEvalSettings;
            if (other == null) { return false; }

            var v1 = SerializeObject<ChessEvalSettings>(this);
            var v2 = SerializeObject<ChessEvalSettings>(other);
            return v1 == v2;
        }

        public override int GetHashCode()
        {
            return 1;// SerializeObject<ChessEvalSettings>(this).GetHashCode();
        }


        public static ChessEvalSettings Load(System.IO.Stream stream, bool applyDefaultSettings = true)
        {
            XmlDocument loadDoc = new XmlDocument();
            loadDoc.Load(stream);

            if (applyDefaultSettings)
            {
                ApplyDefaultValuesToXml(loadDoc.SelectSingleNode("ChessEvalSettings") as XmlElement, getDefaultDoc().SelectSingleNode("ChessEvalSettings") as XmlElement);
            }

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(ChessEvalSettings));

            using (var memory = new System.IO.MemoryStream())
            {
                loadDoc.Save(memory);
                memory.Position = 0;
                return (ChessEvalSettings)xs.Deserialize(memory);
            }
        }

        public static ChessEvalSettings Load(System.IO.FileInfo file, bool applyDefaultSettings = true)
        {
            using (var stream = file.OpenRead())
            {
                return Load(stream);
            }
        }

        public void Save(System.IO.Stream stream)
        {
            using (var writer = new System.IO.StreamWriter(stream))
            {
                writer.Write(SerializeObject<ChessEvalSettings>(this));
            }
        }
        public void Save(System.IO.FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }
            using (var stream = file.OpenWrite())
            {
                Save(stream);
            }
        }
        public void Save(string fileName)
        {
            Save(new System.IO.FileInfo(fileName));
        }


        private static void ApplyDefaultValuesToXml(XmlElement ele, XmlElement defEle)
        {
            foreach (XmlNode nodeChildDefault in defEle.ChildNodes)
            {
                if (nodeChildDefault is XmlElement)
                {
                    XmlElement eleChildDefault = nodeChildDefault as XmlElement;
                    XmlElement match = ele.SelectSingleNode(eleChildDefault.Name) as XmlElement;
                    if (match == null)
                    {
                        match = ele.OwnerDocument.CreateElement(eleChildDefault.Name);
                        ele.AppendChild(match);
                    }
                    ApplyDefaultValuesToXml(match, eleChildDefault);
                }
                else if(nodeChildDefault is XmlText)
                {
                    if (ele.InnerXml == "")
                    {
                        ele.InnerXml = defEle.InnerXml;
                    }                    
                }
                
            }
            if (defEle.ChildNodes.Count == 0)
            {
                
            }
        }

        private static XmlDocument _defaultDoc;
        private static XmlDocument getDefaultDoc()
        {
            if (_defaultDoc == null)
            {
                _defaultDoc = new XmlDocument();
                _defaultDoc.LoadXml(SerializeObject<ChessEvalSettings>(ChessEvalSettings.Default()));
            }
            return _defaultDoc;
        }

        /// <summary>
        /// Serialize an object into an XML string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T obj)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

                System.IO.StreamWriter writer = new System.IO.StreamWriter(memoryStream, Encoding.UTF8);

                xs.Serialize(writer, obj);
                memoryStream.Position = 0;
                System.IO.StreamReader reader = new System.IO.StreamReader(memoryStream, Encoding.UTF8);
                string retval = reader.ReadToEnd();
                return retval;
                //return reader.ReadToEnd();
                //xmlString = UTF8ByteArrayToString(memoryStream.ToArray()); 
                //return xmlString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reconstruct an object from an XML string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(memoryStream, Encoding.UTF8);
            writer.Write(xml);
            writer.Flush();




            memoryStream.Position = 0;


            System.IO.StreamReader reader = new System.IO.StreamReader(memoryStream, Encoding.UTF8);
            return (T)xs.Deserialize(reader);
        }


        

    }

}

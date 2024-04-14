using Martridge.Models.Configuration.Save;
using System.Collections.Generic;

namespace Martridge.Models.Configuration {
    public static class InstallerDefaults {
        #region DINK HD
        
        public static ConfigDataInstaller DinkHD_LatestRtSoft() {
            return new ConfigDataInstaller() {
                Category = "Dink HD",
                Name = "DinkHD",
                DestinationName = "DinkHD",
                GameFileName = "dink.exe",
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "",
                            CheckSha256 = false,
                            Name = "dink_smallwood_hd.exe",
                            Uri = @"https://www.rtsoft.com/dink/DinkSmallwoodHDInstaller.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigDataInstaller DinkHD_TDN_V1_93() {
            return new ConfigDataInstaller() {
                Category = "Dink HD",
                Name = "DinkHD V1.93",
                DestinationName = "DinkHD_V1.93",
                GameFileName = "dink.exe",
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "fd451463caaf05c6f321850d6a1c3c2f3dfe2579152934fcce4b441c4ead1efb",
                            CheckSha256 = true,
                            Name = "dink_smallwood_hd-v1_9_3.exe",
                            Uri = @"https://www.dinknetwork.com/download/dink_smallwood_hd-v1_9_3.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigDataInstaller DinkHD_TDN_V1_97() {
            return new ConfigDataInstaller() {
                Category = "Dink HD",
                Name = "DinkHD V1.97",
                DestinationName = "DinkHD_V1.97",
                GameFileName = "dink.exe",
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "ed3a2243df9dd9e89239206ada77decf2afa32dfb13515670e93099465d7a575",
                            CheckSha256 = true,
                            Name = "dink_smallwood_hd-1_97.exe",
                            Uri = @"https://www.dinknetwork.com/download/dink_smallwood_hd-1_97.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigDataInstaller DinkHD_TDN_V1_98() {
            return new ConfigDataInstaller() {
                Category = "Dink HD",
                Name = "DinkHD V1.98",
                DestinationName = "DinkHD_V1.98",
                GameFileName = "dink.exe",
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "1e6a76557955cb90f92a4102caf1ee6a8e7cc8f1c0815056e3420ac0bda290d4",
                            CheckSha256 = true,
                            Name = "dink_smallwood_hd-1_98.exe",
                            Uri = @"https://www.dinknetwork.com/download/dink_smallwood_hd-1_98.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigDataInstaller DinkHD_TDN_V1_99() {
            return new ConfigDataInstaller() {
                Category = "Dink HD",
                Name = "DinkHD V1.99",
                DestinationName = "DinkHD_V1.99",
                GameFileName = "dink.exe",
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "e4d64dcf6a074abb14c6e2c4312762a74719a3497e76975380194d35311c73a6",
                            CheckSha256 = true,
                            Name = "dink_smallwood_hd-1_99.exe",
                            Uri = @"https://www.dinknetwork.com/download/dink_smallwood_hd-1_99.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        #endregion

        #region DINK Classic

        public static ConfigDataInstaller DinkClassic_V1_08() {
            return new ConfigDataInstaller() {
                Category = "Dink Classic",
                Name = "Dink V1.08",
                DestinationName = "Dink_V1.08",
                GameFileName = "dink.exe",
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "d6be28b01602813df02439d4d2974bd38820dc9ce0ea71da5110fc825a360d8e",
                            CheckSha256 = true,
                            Name = "dinksmallwood108.exe",
                            Uri = @"https://www.dinknetwork.com/download/dinksmallwood108.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        
        /*
        // NOTE: Dink V1.07 Beta 3 and Dink V1.06 use a different installer than NSIS
        // currently the installer handler can not actually extract the files from it... Oups! 
         
        public static ConfigDataInstaller DinkClassic_V1_07B3() {
            return new ConfigDataInstaller() {
                Category = "Dink Classic",
                Name = "Dink V1.07 Beta3",
                GameFileName = "dink.exe",
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "0f770ecd14c7cfb520d1a27ee3188acf165edf70b016c038e6b3d61452ea432c",
                            CheckSha256 = true,
                            Name = "dink_smallwood-1_07_beta_3.exe",
                            Uri = @"https://www.dinknetwork.com/download/dink_smallwood-1_07_beta_3.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigDataInstaller DinkClassic_V1_06() {
            return new ConfigDataInstaller() {
                Category = "Dink Classic",
                Name = "Dink V1.06",
                GameFileName = "dink.exe",
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "2de284e2110358a544e9f60ed85cce855fbdeb337c13dcd4f6d3d40a8fb0e578",
                            CheckSha256 = true,
                            Name = "dinksmallwood106.exe",
                            Uri = @"https://www.dinknetwork.com/download/dinksmallwood106.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        */

        #endregion

        #region YEOLDEDINK
        
        public static ConfigDataInstaller YeOldeDink_V062_With_V108Data_And_Localizations() {
            return new ConfigDataInstaller() {
                Category = "YeOldeDink",
                Name = "YeOldeDink V0.6.2",
                DestinationName = "yeoldedink062",
                GameFileName = "yedink062.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    // The DinkV108 data
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "d6be28b01602813df02439d4d2974bd38820dc9ce0ea71da5110fc825a360d8e",
                            CheckSha256 = true,
                            Name = "dinksmallwood108.exe",
                            Uri = @"https://www.dinknetwork.com/download/dinksmallwood108.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.UseWhiteList,
                        FileFilterList = new List<string>() {
                            @"dink",
                            @"island",
                        },
                    },
                    // The Ye Olde Dink
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "472ddef0cea5daa824f4f68e48871e836b2b83ecdbff947a462472c9c2dc49f7",
                            CheckSha256 = true,
                            Name = "yeoldedinkV0.6.2.7z",
                            Uri = @"https://files.catbox.moe/tx41ug.7z",
                            ResourceArchiveFormat = "SevenZip",
                        },
                        FileFilterMode = InstallerFiltering.NoFiltering,
                    },
                },
            };
        }

        public static ConfigDataInstaller YeOldeDink_V05_With_V108Data_And_Localizations() {
            return new ConfigDataInstaller() {
                Category = "YeOldeDink",
                Name = "YeOldeDink V0.5",
                DestinationName = "yeoldedink05",
                GameFileName = "yedink05.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    // The DinkV108 data
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "d6be28b01602813df02439d4d2974bd38820dc9ce0ea71da5110fc825a360d8e",
                            CheckSha256 = true,
                            Name = "dinksmallwood108.exe",
                            Uri = @"https://www.dinknetwork.com/download/dinksmallwood108.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.UseWhiteList,
                        FileFilterList = new List<string>() {
                            @"dink",
                            @"island",
                        },
                    },
                    // The Ye Olde Dink
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "fe5dc1462908b7ba79688b519b0e5c1425bf34ece45f29a2eb06b756e6ec3a3e",
                            CheckSha256 = true,
                            Name = "yeoldedinkV0.5.7z",
                            Uri = @"https://files.catbox.moe/oi5vnz.7z",
                            ResourceArchiveFormat = "SevenZip",
                        },
                        SourceSubFolder = "yeoldedink05",
                        FileFilterMode = InstallerFiltering.NoFiltering,
                    },
                },
            };
        }

        #endregion

        #region FREEDINK
        
        public static ConfigDataInstaller FreeDink_V1_09_6_With_V108Data_And_Localizations() {
            return new ConfigDataInstaller() {
                Category = "Freedink",
                Name = "FreeDink V1.09.6",
                DestinationName = "FreeDink_V1.09.6",
                GameFileName = "freedink.exe",
                EditorFileName = "freedinkedit.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    // The DinkV108 data
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "d6be28b01602813df02439d4d2974bd38820dc9ce0ea71da5110fc825a360d8e",
                            CheckSha256 = true,
                            Name = "dinksmallwood108.exe",
                            Uri = @"https://www.dinknetwork.com/download/dinksmallwood108.exe",
                            ResourceArchiveFormat = "Nsis",
                        },

                        FileFilterMode = InstallerFiltering.UseWhiteList,
                        FileFilterList = new List<string>() {
                            @"dink",
                            @"island",
                        },
                    },
                    // Freedink V1.09.6
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "9f42039929251027412dbb5247c299de7c74dff5854f0dd03abc3c10bc3293c2",
                            CheckSha256 = true,
                            Name = "gnu_freedink-109_6.zip",
                            Uri = @"https://www.dinknetwork.com/download/gnu_freedink-109_6.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                    // FreeDinkData - localizations only
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "7cb32cae1d88fd15d2c441d6cbfe11e418bab6d66d84e2abe8a1b0c46bbd527a",
                            CheckSha256 = true,
                            Name = "freedink_data-1_08_20190120.zip",
                            Uri = @"https://www.dinknetwork.com/download/freedink_data-1_08_20190120.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.UseWhiteList,
                        FileFilterList = new List<string>() {
                            @"dink\l10n",
                            @"licenses",
                            @"freedink-data-COPYING.txt",
                            @"freedink-data-NEWS.txt",
                            @"freedink-data-README.txt",
                            @"freedink-data-README-REPLACEMENTS.txt",
                        },
                    },
                },
            };
        }

        public static ConfigDataInstaller Freedink_V1_09_6_With_GnuData() {
            return new ConfigDataInstaller() {
                Category = "Freedink",
                Name = "FreeDink V1.09.6 (GNU Assets)",
                DestinationName = "FreeDink_V1.09.6_GNUAssets",
                GameFileName = "freedink.exe",
                EditorFileName = "freedinkedit.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    // Freedink V1.09.6
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "9f42039929251027412dbb5247c299de7c74dff5854f0dd03abc3c10bc3293c2",
                            CheckSha256 = true,
                            Name = "gnu_freedink-109_6.zip",
                            Uri = @"https://www.dinknetwork.com/download/gnu_freedink-109_6.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                    // FreeDinkData - localizations only
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "7cb32cae1d88fd15d2c441d6cbfe11e418bab6d66d84e2abe8a1b0c46bbd527a",
                            CheckSha256 = true,
                            Name = "freedink_data-1_08_20190120.zip",
                            Uri = @"https://www.dinknetwork.com/download/freedink_data-1_08_20190120.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }

        #endregion

        #region WinDinkedit

        public static ConfigDataInstaller WinDinkEdit_V1_4B2() {
            return new ConfigDataInstaller() {
                Category = "WinDinkEdit",
                Name = "WinDinkEdit V1.4 Beta2",
                DestinationName = "WinDinkEdit_V1.4_Beta2",
                EditorFileName = "WinDinkedit.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "c03d4ea6b39860d92033da03f7b609ae1fa0bcef414e35d1b97025acbed615c6",
                            CheckSha256 = true,
                            Name = "windinkedit10.zip",
                            Uri = @"https://www.dinknetwork.com/download/develop/windinkedit10.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigDataInstaller WinDinkEditPlus_V1_2() {
            return new ConfigDataInstaller() {
                Category = "WinDinkEdit",
                Name = "WinDinkEditPlus V1.2",
                DestinationName = "WinDinkEditPlus_V1.2",
                EditorFileName = "WinDinkeditPlus.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "cba9d1336b2531585f78f653c1c4bc3f9d88044ae0ed66f45b39e3fe5cded183",
                            CheckSha256 = true,
                            Name = "windinkedit_plus-1_2",
                            Uri = @"https://www.dinknetwork.com/download/windinkedit_plus-1_2.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigDataInstaller WinDinkEditPlus2_V2_3_2() {
            return new ConfigDataInstaller() {
                Category = "WinDinkEdit",
                Name = "WinDinkEditPlus V2.3.2",
                DestinationName = "WinDinkEditPlus_V2.3.2",
                EditorFileName = "WinDinkeditPlus2.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "5ce992febfdcf89fa3d8437c01bacc2ee0d87f43a3af13ac92f87a79da7c0815",
                            CheckSha256 = true,
                            Name = "windinkedit_plus_2-2_3_2.zip",
                            Uri = @"https://www.dinknetwork.com/download/windinkedit_plus_2-2_3_2.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        public static ConfigDataInstaller WinDinkEditPlus2_V2_5() {
            return new ConfigDataInstaller() {
                Category = "WinDinkEdit",
                Name = "WinDinkEditPlus V2.5",
                DestinationName = "WinDinkEditPlus_V2.5",
                EditorFileName = "WinDinkeditPlus2.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "629ee6d784514f2ea78c6ee68fde17ec4bc467a04868477b2c1389d07da67ae8",
                            CheckSha256 = true,
                            Name = "windinkedit_plus_2-2_5.zip",
                            Uri = @"https://www.dinknetwork.com/download/windinkedit_plus_2-2_5.zip",
                            ResourceArchiveFormat = "Zip",
                        },

                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }
        
        #endregion
        
        #region WinDinkedit - drone edition
        
        public static ConfigDataInstaller WinDinkEditPlus2_V2_5_7_8() {
            return new ConfigDataInstaller() {
                Category = "WinDinkEditDrone",
                Name = "WinDinkEditPlusDrone V2.5.7.8",
                DestinationName = "WDED_V2.5.7.8",
                EditorFileName = "WinDinkeditPlus2.exe",
                
                InstallerComponents = new List<ConfigDataInstallerComponent>() {
                    new ConfigDataInstallerComponent() {
                        WebResource = new ConfigDataWebResource() {
                            Sha256 = "3496150c568c3eaab3217fea5638976818d68b4fb71deeda337e97d39f63d5bb",
                            CheckSha256 = true,
                            Name = "windinkedit_plus_2_drone_edition-v2_5_7_8.zip",
                            Uri = @"https://www.dinknetwork.com/download/windinkedit_plus_2_drone_edition-v2_5_7_8.zip",
                            ResourceArchiveFormat = "Zip",
                        },
                        SourceSubFolder = "WDED_V2.5.7.8",
                        FileFilterMode = InstallerFiltering.NoFiltering,
                        FileFilterList = new List<string>(),
                    },
                },
            };
        }

        #endregion
        

        
    }
}

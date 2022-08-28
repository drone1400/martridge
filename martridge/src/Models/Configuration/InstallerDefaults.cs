using Martridge.Models.Configuration.Save;
using System.Collections.Generic;

namespace Martridge.Models.Configuration {
    public static class InstallerDefaults {
        #region DINK HD
        
        public static ConfigDataInstaller DinkHD_LatestRtSoft() {
            return new ConfigDataInstaller() {
                Category = "Dink HD",
                Name = "Dink HD",
                ApplicationFileName = "dink.exe",
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
                Name = "Dink HD V1.93",
                ApplicationFileName = "dink.exe",
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
                Name = "Dink HD V1.97",
                ApplicationFileName = "dink.exe",
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
                Name = "Dink HD V1.98",
                ApplicationFileName = "dink.exe",
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
                Name = "Dink HD V1.99",
                ApplicationFileName = "dink.exe",
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
                ApplicationFileName = "dink.exe",
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
        
        public static ConfigDataInstaller DinkClassic_V1_07B3() {
            return new ConfigDataInstaller() {
                Category = "Dink Classic",
                Name = "Dink V1.07 Beta3",
                ApplicationFileName = "dink.exe",
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
                ApplicationFileName = "dink.exe",
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

        #endregion

        #region FREEDINK
        
        public static ConfigDataInstaller FreeDink_V1_09_6_With_V108Data_And_Localizations() {
            return new ConfigDataInstaller() {
                Category = "Freedink",
                Name = "FreeDink V1.09.6",
                ApplicationFileName = "freedink.exe",
                
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
                Name = "FreeDink V1.09.6 (GNU assets)",
                ApplicationFileName = "freedink.exe",
                
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
                Name = "WinDinkEdit V1.4 Beta 2",
                ApplicationFileName = "WinDinkedit.exe",
                
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
                Name = "WinDinkEdit Plus V1.2",
                ApplicationFileName = "WinDinkeditPlus.exe",
                
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
                Name = "WinDinkEdit Plus V2.3.2",
                ApplicationFileName = "WinDinkeditPlus2.exe",
                
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

        #endregion

        
    }
}

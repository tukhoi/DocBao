using Davang.Parser.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Davang.Utilities.Extensions;

namespace DocBao.ApplicationServices.Bank
{
    public static class CategoryBank
    {
        public static IList<Category> Categories { get; private set; }

        static CategoryBank()
        {
            Initialize();
        }

        /// <summary>
        /// This is feed based app
        /// All things should based on subscribed feeds
        /// </summary>
        private static void Initialize()
        {
            

            Categories = GetUserDefinedCategories() ?? GetPredefinedCategories();
        }

        private static IList<Category> GetUserDefinedCategories()
        {
            return null;
        }

        private static IList<Category> GetPredefinedCategories()
        {
            var feedManager = FeedManager.GetInstance();
            return new List<Category>
            {
                new Category() { 
                    Id = new Guid("339435fe-10f3-498c-bef1-6f967bfab7a9"),
                    Name = "tin mới",
                    ImageUri = new Uri("/Images/categories/tinmoi.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("5b117b23-af7e-432f-829b-2211c3f4cda7"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("fc74fccc-03c2-452f-86ca-50516e970afb"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("e0fab5b4-1636-44ce-9d4e-13a77d03b196"), true).Target,   //vietnamnet
                        feedManager.GetSubscribedFeed(new Guid("a947f475-b7c6-48ca-b279-a3127528702b"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("c16392f2-9918-4a2f-a6a3-7e0b1bf9c395"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("48a3682e-9d96-4c16-90b3-de0d75d85a43"), true).Target,   //kenh14
                        feedManager.GetSubscribedFeed(new Guid("3abdcd4f-46f4-4a21-bd30-f6671c1cd654"), true).Target,   //kienthucngaynay
                        feedManager.GetSubscribedFeed(new Guid("4d5827f7-504b-494b-9176-c3c5dd10cad8"), true).Target,   //tuoitre
                        feedManager.GetSubscribedFeed(new Guid("9218f218-75dc-4237-8491-e2ce547ee7f7"), true).Target,   //laodong
                        //feedManager.GetSubscribedFeed(new Guid("7bbb39d1-49e3-4078-be50-695449a82340"), true).Target    //nguoilaodong
                    }
                },
                new Category() { 
                    Id = new Guid("d8c2386a-0cc2-405e-9bf4-ba508e1ed29c"),
                    Name = "thế giới",
                    ImageUri = new Uri("/Images/categories/thegioi.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("18904d31-17bc-4547-bf3c-961239ed6280"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("2502b8ff-36e8-4f3a-93f3-b8c8c695debd"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("f4ce37c9-f70d-4cde-b137-54a0a17cfaf1"), true).Target,   //vietnamnet
                        feedManager.GetSubscribedFeed(new Guid("aea81c37-a685-462e-a4aa-bf20614507a0"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("c4dd69d9-d907-46a3-b7e7-625d2e7511c9"), true).Target,   //thethaovanhoa                        
                        feedManager.GetSubscribedFeed(new Guid("1419928c-4ae6-42aa-92c2-a9e955f52a71"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("66a0c752-ab0d-4cbc-968a-76345f594627"), true).Target,   //tuoitre
                        feedManager.GetSubscribedFeed(new Guid("0c6bb214-047d-451c-970e-1cc27a54b690"), true).Target,   //laodong
                        //feedManager.GetSubscribedFeed(new Guid("493dc72b-5320-4457-b58b-7fc4255060a3"), true).Target,   //nguoilaodong
                    }
                },
                new Category() { 
                    Id = new Guid("617292b9-1580-4504-9cca-602de10ef02f"),
                    Name = "kinh tế",
                    ImageUri = new Uri("/Images/categories/kinhte.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("d129bd55-e322-4024-b36b-fe09a33eedd9"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("353b7e91-aa47-4975-b1e1-db2b128ef5d8"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("177c6821-b002-4b3c-8e06-feaacbb5bdb4"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("96c697c5-1d31-485c-8204-b94a224f1469"), true).Target,   //vietnamnet
                        feedManager.GetSubscribedFeed(new Guid("b4bfed8d-8692-438b-ae4d-713024738240"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("bd50d029-1190-4aee-941a-c1f8631f63ab"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("b84dd60e-d9d4-4be4-8806-434b91872981"), true).Target,   //cafef
                        feedManager.GetSubscribedFeed(new Guid("3ed827a1-3dc9-48d9-b8bf-320b9bfa9902"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("c370f95b-6c4c-4200-b105-15186cffc92c"), true).Target,   //tuoitre
                        feedManager.GetSubscribedFeed(new Guid("ddc4edf8-de1c-4a32-a05c-d9c4c3987229"), true).Target,   //laodong
                        //feedManager.GetSubscribedFeed(new Guid("e75f5d05-f00d-4db7-9ad5-1f0389d96c02"), true).Target,   //nguoilaodong
                    }
                },
                new Category() { 
                    Id= new Guid("c96e3b84-795c-490e-94bc-f6813ac75eb1"),
                    Name = "xã hội",
                    ImageUri = new Uri("/Images/categories/xahoi.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("c8420b6a-afa0-4f6e-a616-fcf03379359d"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("9f9b125f-8fc3-4607-9ef4-b53c67764e04"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("4d883ba0-c34e-4e0c-a06b-249f1b5f4621"), true).Target,   //vietnamnet
                        feedManager.GetSubscribedFeed(new Guid("35a4eb83-2130-4d71-b937-e054e06999b9"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("7d9fd604-311b-42b7-a6e7-708d106c2577"), true).Target,   //thethaovanhoa                        
                        feedManager.GetSubscribedFeed(new Guid("03e99378-97c1-4e64-a282-8dd1c0e76df2"), true).Target,   //giadinh
                        feedManager.GetSubscribedFeed(new Guid("bc3384d7-5e2d-42ff-b286-f9c3ea8eb61e"), true).Target,   //kienthucngaynay
                        feedManager.GetSubscribedFeed(new Guid("1bd4cfed-dd5d-42a2-8a2d-86af12db00cd"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("122c60e4-2e09-40bc-bb9a-08b83b51df37"), true).Target,   //laodong
                        //feedManager.GetSubscribedFeed(new Guid("681c25eb-b614-4b4e-8197-d79df14b0597"), true).Target,   //nguoilaodong
                    }
                },
                new Category() { 
                    Id = new Guid("e11f75f5-5f91-494d-a1a5-a3ad354e28dc"),
                    Name = "thể thao",
                    ImageUri = new Uri("/Images/categories/thethao.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("e559f4cf-067c-4d08-98ed-5d897050972f"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("ab7cd32e-d2e4-4f50-8768-594781acde8c"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("3b85f3e2-84fe-48a4-a4b2-6f92dd368636"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("6faa6131-9fe3-41ee-9340-9432cddd6086"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("096bc270-6029-403f-a375-39c00ca10d8e"), true).Target,   //ngoisao
                        feedManager.GetSubscribedFeed(new Guid("83c959fd-9440-4210-90e1-4bce75228121"), true).Target,   //thehthaovanhoa
                        feedManager.GetSubscribedFeed(new Guid("8dd3153c-5ed1-414f-aca7-d2acd0615dc6"), true).Target,   //bongda
                        feedManager.GetSubscribedFeed(new Guid("8d7cc1c4-0526-4b25-9b6e-a6cf5d65183b"), true).Target,   //kienthucngaynay
                        feedManager.GetSubscribedFeed(new Guid("c4610f84-1223-44d1-a7a7-fad6a804793f"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("3e49146c-6adb-4fcc-be10-9b2a2d6a0e65"), true).Target,   //laodong
                        //feedManager.GetSubscribedFeed(new Guid("efb3dae7-ace1-4f02-ae38-f891f1abdde9"), true).Target,   //nguoilaodong
                    }
                },
                new Category() { 
                    Id = new Guid("06a1bc28-70f2-42d0-b6d2-5a3e59df667c"),
                    Name = "văn hóa",
                    ImageUri = new Uri("/Images/categories/vanhoa.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("557c322c-069d-4528-89e9-3792517fcbca"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("6dad63e2-c66a-4eed-aafa-d7e6dd374897"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("b5ffcf9d-b0d2-4bde-8a68-f5e41c50e570"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("b3dfff0d-80b3-4473-a301-45e8914906ad"), true).Target,   //vietnamnet
                        feedManager.GetSubscribedFeed(new Guid("8b658067-0b5e-45ed-8e99-e0d1c16af0d9"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("0cf76bce-bebe-4ce8-9214-ff590fdf8dcc"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("f2ced884-4028-4a75-b05d-281bca0a6e1e"), true).Target,   //kienthucngaynay
                        feedManager.GetSubscribedFeed(new Guid("afb7443b-8a5e-4e82-975a-414372c6ecf9"), true).Target,   //tuoitre
                        feedManager.GetSubscribedFeed(new Guid("c0788969-450e-43af-ac02-b49eab460651"), true).Target,   //tuoitre
                        feedManager.GetSubscribedFeed(new Guid("4f3c232e-e1de-456f-9fa1-0932c674fd0d"), true).Target,   //laodong
                        //feedManager.GetSubscribedFeed(new Guid("7792d26c-3857-47bf-920c-0e58f8f74dc6"), true).Target,   //nguoilaodong
                    }
                },
                new Category() { 
                    Id = new Guid("9bd602ef-6100-4be4-9a79-733c6fa5a872"),
                    Name = "giải trí",
                    ImageUri = new Uri("/Images/categories/giaitri.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("dddff8c4-54ad-4a38-b016-144a004c967f"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("6fae8c71-5103-4af7-b2ec-66b8f64797e6"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("dafc4806-9c62-4d6b-8608-427ae6be6551"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("4c35e67e-1ff5-48d3-a5a0-41a4148dc226"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("e5dfa40c-55d4-4fa5-b79d-9d6629faed0e"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("a4187888-dae1-4bf3-85fc-06d8e22fe076"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("a6229ccb-b035-4554-ac4a-f97a98110a44"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("e74d9400-2a8e-4fc0-a185-06499516de57"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("94ebc489-93a7-431b-8f49-3222e5f8230f"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("0ed5c415-ea7d-471e-b37c-2371fe03df93"), true).Target,   //ngoisao
                        feedManager.GetSubscribedFeed(new Guid("ad74eee0-631b-44b1-a7ad-274e78c0d576"), true).Target,   //ngoisao
                        feedManager.GetSubscribedFeed(new Guid("7978c6dc-57a2-4d60-bca8-a24e49a12e9b"), true).Target,   //afamily
                        feedManager.GetSubscribedFeed(new Guid("da839813-900c-4705-9395-d451161170f8"), true).Target,   //afamily
                        feedManager.GetSubscribedFeed(new Guid("38d177fc-6e5c-4c64-b6b7-59eae5fa4ff4"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("09df6e4c-da83-4019-a574-e9d167435f79"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("7a2fcf1d-e359-497b-ae86-4ec80d3bf853"), true).Target,   //2sao
                        feedManager.GetSubscribedFeed(new Guid("29f4b0a9-cd49-4c58-a43b-a8efb6096cc9"), true).Target,   //2sao
                        feedManager.GetSubscribedFeed(new Guid("896ad11e-1db6-42ea-9afe-67456f92eb88"), true).Target,   //2sao
                        feedManager.GetSubscribedFeed(new Guid("36221c2e-323a-4fa7-8c0a-efc197cc2b14"), true).Target,   //2sao
                    }
                },
                new Category() { 
                    Id = new Guid("5ef0fae5-8dee-4715-a9f1-68df5d698847"),
                    Name = "khoa học công nghệ",
                    ImageUri = new Uri("/Images/categories/khcn.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("e7977f23-e798-417a-99ac-7fc561026eeb"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("3b0b1801-d743-4191-b38d-417ce9bedd27"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("c6d83f98-deec-4865-ba0a-69b15b539a3b"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("f1b00803-2be2-4a6e-84aa-cde86f64622d"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("615f0f59-bad5-4045-84b6-828e12bbac7f"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("1eed0e7b-0793-4e28-918f-0811e1c7f135"), true).Target,   //vietnamnet
                        feedManager.GetSubscribedFeed(new Guid("370d600e-02bd-4c78-85f3-ffb926d31ea8"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("5b73c2a5-90e5-4a3e-8800-3143f6d8d891"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("a0f08840-0859-44cc-bdea-9f0349a71495"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("04aebaee-be2c-46c8-ad4e-6041041d9736"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("a417c78c-6d53-4d46-9658-bd9aa520e6e7"), true).Target,   //2sao
                        feedManager.GetSubscribedFeed(new Guid("1d44c727-d875-45fe-87b4-457870584791"), true).Target,   //laodong
                        //feedManager.GetSubscribedFeed(new Guid("c19c977d-7e2a-4bdd-a02a-955a71515ca7"), true).Target,   //nguoilaodong
                    }
                },
                new Category() { 
                    Id = new Guid("16e79392-19a1-4f7c-a72f-620a123e8e9d"),
                    Name = "ôtô xe máy",
                    ImageUri = new Uri("/Images/categories/otoxemay.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("6a6bbc3b-5c15-4053-9b06-0506c3ba5084"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("21c78310-489f-419b-9c4a-d83bd1e30b45"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("fe8558e2-2b35-4409-926a-88d1974af799"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("2df0caf8-cad6-46af-9901-ceee77e67784"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("be9be699-75cb-4838-afcf-be34245e97d1"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("3e3a717a-ef3c-46a1-8a3b-7359a4be95b5"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("3aed472f-62d9-41ff-af35-df0428450edc"), true).Target,   //dantri
                        feedManager.GetSubscribedFeed(new Guid("632c4584-ff9e-49c9-8236-fafa6822f3c9"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("c52566e5-f97a-4994-ae2d-0a3f02e5987d"), true).Target,   //kienthucngaynay
                        feedManager.GetSubscribedFeed(new Guid("2ec0816b-90f7-4e40-a20a-0a07772a7230"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("b9180784-6144-485c-9e62-74d3011ac446"), true).Target,   //doisongphapluat
                        feedManager.GetSubscribedFeed(new Guid("bb8ad746-2032-45f6-b08d-ab483b8a9161"), true).Target,   //tuoitre
                    }
                },
                new Category()
                {
                    Id = new Guid("d2c5285a-9594-44b7-9a75-35770cc9633e"),
                    Name = "cười",
                    ImageUri = new Uri("/Images/categories/cuoi.png", UriKind.Relative),
                    Feeds = new List<Feed>
                    {
                        feedManager.GetSubscribedFeed(new Guid("86dc31e7-721c-411c-8a0f-108094d5dcd7"), true).Target,   //vnexpress
                        feedManager.GetSubscribedFeed(new Guid("298a3314-00a0-4550-982d-93ab3c5c227e"), true).Target,   //thanhnien
                        feedManager.GetSubscribedFeed(new Guid("f46b4627-8418-47f4-9372-8b4e2daec3d5"), true).Target,   //haivl
                        feedManager.GetSubscribedFeed(new Guid("24340dc3-a088-4f4c-99f8-8d82f1ac5d19"), true).Target,   //24h
                        feedManager.GetSubscribedFeed(new Guid("c60422cb-8ca4-480e-87c8-acd080d7a208"), true).Target,   //ngoisao
                        feedManager.GetSubscribedFeed(new Guid("db54bfb3-ff6a-41dd-9eca-30b8c487cd47"), true).Target,   //thethaovanhoa
                        feedManager.GetSubscribedFeed(new Guid("a67c7e4e-0e51-460e-be66-e5e31439684e"), true).Target,   //kienthucngaynay
                        feedManager.GetSubscribedFeed(new Guid("ba9c4c47-7773-441e-8209-5f8079e4af3c"), true).Target,   //kienthucngaynay
                        feedManager.GetSubscribedFeed(new Guid("38b33a74-0619-4626-98b6-ffd25a54ecb3"), true).Target,   //doisongphapluat
                    }
                }
            };
        }
    }
}

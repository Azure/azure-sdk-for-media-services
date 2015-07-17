//-----------------------------------------------------------------------
// <copyright file="AssetFilterTests.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class AssetFilterTests
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void AssetFilterCRUD()
        {
            IStreamingFilter filter = _mediaContext.Filters.Create("UniTest", new PresentationTimeRange(), new List<FilterTrackSelectStatement>());
            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.Tracks);
            filter.Delete();
            Assert.IsNull(_mediaContext.Assets.Where(c => c.Name == filter.Name).FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldFailForEmptyName()
        {
            IStreamingFilter filter = _mediaContext.Filters.Create(String.Empty, new PresentationTimeRange(), new List<FilterTrackSelectStatement>());
        }

        [TestMethod]
        public void ShouldNotValidateNonEmptyName()
        {
            StringBuilder bld = new StringBuilder();
            
            //Generating long string
            for (int i = 0; i < 10; i++)
            {
                bld.Append(Guid.NewGuid());
            }
            IStreamingFilter filter = _mediaContext.Filters.Create(bld.ToString(), new PresentationTimeRange(), new List<FilterTrackSelectStatement>());

            bld.Clear();
            bld.Append("treasure island");
            bld.Append("остров сокровищ");
            bld.Append("金银岛");
            bld.Append("Schatzinsel");
            bld.Append("कोष द्विप");
            bld.Append("جزيرة الكنز");
            bld.Append("!@#$%^&&**()_+?>");
            filter = _mediaContext.Filters.Create(bld.ToString(), new PresentationTimeRange(), new List<FilterTrackSelectStatement>());
        }

        [TestMethod]
        public void ShouldNotValidateNameand4CTracksOnClientSide()
        {
            StringBuilder bld = new StringBuilder();
            bld.Append("treasure island");
            bld.Append("остров сокровищ");
            bld.Append("金银岛");
            bld.Append("Schatzinsel");
            bld.Append("कोष द्विप");
            bld.Append("جزيرة الكنز");
            bld.Append("!@#$%^&&**()_+?>");

            //Generating long string
            for (int i = 0; i < 10; i++)
            {
                bld.Append(Guid.NewGuid());
            }
            List<FilterTrackSelectStatement> filterTrackSelectStatements = new List<FilterTrackSelectStatement>();
            filterTrackSelectStatements.Add(new FilterTrackSelectStatement()
            {
                PropertyConditions = new List<IFilterTrackPropertyCondition>()
                {
                    new FilterTrackNameCondition(trackName:bld.ToString())
                }
            });
            filterTrackSelectStatements.Add(new FilterTrackSelectStatement()
            {
                PropertyConditions = new List<IFilterTrackPropertyCondition>()
                {
                    new FilterTrackFourCCCondition(Guid.NewGuid().ToString())
                }
            });
            filterTrackSelectStatements.Add(new FilterTrackSelectStatement()
            {
                PropertyConditions = new List<IFilterTrackPropertyCondition>()
                {
                    new FilterTrackLanguageCondition(Guid.NewGuid().ToString())
                }
            });
            filterTrackSelectStatements.Add(new FilterTrackSelectStatement()
            {
                PropertyConditions = new List<IFilterTrackPropertyCondition>()
                {
                    new FilterTrackBitrateRangeCondition(new FilterTrackBitrateRange())
                }
            });

            IStreamingFilter filter = _mediaContext.Filters.Create(bld.ToString(), new PresentationTimeRange(), filterTrackSelectStatements);

           
        }
 
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ControlUnit.Models;
using log4net;

namespace ControlUnit.Data
{
    internal class DataProvider
    {
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IList<SyndromModel> FindGlobalOpinion(string MasterAgentIp)
        {
            try
            {
                var opinionDB = new SectionsEntities();
                List<Opinion> opinionList = opinionDB.Opinion.Where(x => x.MasterAgentIp == MasterAgentIp).ToList();
                return
                    opinionList.Select(
                        opinion =>
                        new SyndromModel
                            {Hosts = opinion.Hosts, MasterAgentIp = opinion.MasterAgentIp, Syndrom = opinion.Syndrom}).
                        ToList();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            return new List<SyndromModel>();
        }


        public IList FindAllSections()
        {
            try
            {
                var sectionDB = new SectionsEntities();
                List<Test> testList = sectionDB.Test.ToList();
                return testList.Select(test => new TestModel
                    {
                        TestId = test.TestID,
                        IpDest = test.IPDest,
                        IpSrc = test.IPSrc,
                        MasterAgentIp = test.MasterAgentIP,
                        Number = test.Number,
                        Status = "[Brak]",
                        ControlUnitIp = test.ControlUnitIP
                    }).ToList();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            return new List<TestModel>();
        }

        public void DeleteSection(IList sectionModel)
        {
            var sectionDB = new SectionsEntities();
            foreach (object item in sectionModel)
            {
                var testModel = (TestModel) item;
                IQueryable<Test> testsToDel = from test in sectionDB.Test
                                              where test.MasterAgentIP == testModel.MasterAgentIp
                                              select test;
                IQueryable<Opinion> opinionToDel = from op in sectionDB.Opinion
                                                   where op.MasterAgentIp == testModel.MasterAgentIp
                                                   select op;
                foreach (Opinion op in opinionToDel)
                {
                    sectionDB.DeleteObject(op);
                }
                foreach (Test test in testsToDel)
                {
                    sectionDB.DeleteObject(test);
                }
                sectionDB.SaveChanges();
            }
        }

        public void InsertSyndromModel(SyndromModel syndromModel)
        {
            if(syndromModel.Hosts == "[Brak]")
                return;
            
            var syndromDB = new SectionsEntities();

            var opinion = new Opinion
                {
                    Id = syndromDB.Opinion.NextId(v => v.Id),
                    Hosts = syndromModel.Hosts,
                    MasterAgentIp = syndromModel.MasterAgentIp,
                    Syndrom = syndromModel.Syndrom
                };
            syndromDB.AddToOpinion(opinion);
            syndromDB.SaveChanges();
        }

        public void InsertTestModel(TestModel testModel)
        {
            var sectionDB = new SectionsEntities();

            var test = new Test
                {
                    ID = sectionDB.Test.NextId(v => v.ID),
                    TestID = testModel.TestId,
                    IPDest = testModel.IpDest,
                    IPSrc = testModel.IpSrc,
                    MasterAgentIP = testModel.MasterAgentIp,
                    Number = testModel.Number,
                    ControlUnitIP = testModel.ControlUnitIp
                };

            sectionDB.AddToTest(test);
            sectionDB.SaveChanges();
        }
    }
}
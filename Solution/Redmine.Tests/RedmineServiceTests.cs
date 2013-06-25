using System;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace Redmine.Tests
{
    [TestFixture]
    public class RedmineServiceTests
    {
        RedmineService redmine;

        [SetUp]
        public void SetUp()
        {
            redmine = new RedmineService() {
                Username = "lemon",
                Password = "Pa$sw0rd",
                BaseUriString = "test://redmine/",
            };
        }

        [Test]
        public void GetIssues()
        {
            var request = CreateRequestMock(issuesXml);
            
            var issues = redmine.GetIssues();

            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual("Parse Redmine API XML", issues[0].Subject);
        }

        [Test]
        public void GetIssues_AssertUri()
        {
            var request = CreateRequestMock(issuesXml);
            request.Expect(x => x.Create(new Uri("test://redmine/issues.xml?assigned_to_id=me"))).Repeat.Once();
            
            redmine.GetIssues();

            request.VerifyAllExpectations();
        }

        [Test]
        public void GetIssues_AssertCustomQuery()
        {
            var request = CreateRequestMock(issuesXml);
            request.Expect(x => x.Create(new Uri("test://redmine/issues.xml?assigned_to_id=me&limit=3"))).Repeat.Once();
            
            redmine.GetIssues("assigned_to_id=me&limit=3");

            request.VerifyAllExpectations();
        }


        [Test]
        public void GetProjects()
        {
            var request = CreateRequestMock(projectsXml);
            
            var projects = redmine.GetProjects();

            Assert.AreEqual(1, projects.Length);
            Assert.AreEqual("Redmine Task List", projects[0].Name);
        }

        [Test]
        public void GetProjects_AssertUri()
        {
            var request = CreateRequestMock(projectsXml);
            request.Expect(x => x.Create(new Uri("test://redmine/projects.xml"))).Repeat.Once();
            
            redmine.GetProjects();

            request.VerifyAllExpectations();
        }
        
        [Test]
        public void GetProjects_AssertAllAreRequested()
        {
            var request = CreateRequestMock(projectsXmlCount3Offset0Limit1, projectsXmlCount3Offset1Limit1, projectsXmlCount3Offset2Limit1);
            request.Expect(x => x.Create(new Uri("test://redmine/projects.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/projects.xml?offset=1"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/projects.xml?offset=2"))).Repeat.Once();
            
            var projects = redmine.GetProjects();

            Assert.AreEqual(3, projects.Length);
            request.VerifyAllExpectations();
        }


        private static TestWebRequest CreateRequestMock(params string[] responseXml)
        {
            var request = MockRepository.GenerateMock<TestWebRequest>();
            var response = CreateResponseStub(responseXml);
            
            request.Expect(x => x.GetResponse()).Return(response);
            
            WebRequest.RegisterPrefix("test", TestWebRequest.GetCreator(request));

            return request;
        }

        private static WebResponse CreateResponseStub(params string[] responseXml)
        {
            var response = MockRepository.GenerateStub<WebResponse>();
            var streams = new MemoryStream[responseXml.Length];
            
            for (int i = 0; i < streams.Length; i++)
			{
                streams[i] = new MemoryStream(Encoding.UTF8.GetBytes(responseXml[i]));
			}

            var streamIndex = 0;
            response.Stub(x => x.GetResponseStream()).Do((Func<Stream>)(() => {
                
                if (streamIndex >= streams.Length)
                {
                    throw new InvalidOperationException("Response is not stubbed");
                }

                return streams[streamIndex++];
            }));

            return response;
        }

        
        string issuesXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><issue><id>1</id><project id=\"2\" name=\"Redmine API Library\"/><tracker id=\"2\" name=\"Feature\"/><status id=\"3\" name=\"Resolved\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"Dmitry Popov\"/><assigned_to id=\"1\" name=\"Dmitry Popov\"/><subject>Parse Redmine API XML</subject><description>Users, projects and issues</description><start_date>2013-06-13</start_date><due_date>2013-06-14</due_date><done_ratio>100</done_ratio><estimated_hours>2</estimated_hours><created_on>2013-06-13T22:10:24Z</created_on><updated_on>2013-06-13T22:10:24Z</updated_on><closed_on>2013-06-14T00:15:03Z</closed_on></issue></issues>";
        string projectsXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><project><id>1</id><name>Redmine Task List</name><identifier>redminetasklist</identifier><description></description><created_on>2013-06-13T21:00:00Z</created_on><updated_on>2013-06-13T21:00:00Z</updated_on></project></projects>";
        
        string projectsXmlCount3Offset0Limit1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"3\" offset=\"0\" limit=\"1\" type=\"array\"><project><id>1</id><name>Redmine Task List</name><identifier>redminetasklist</identifier><description></description><created_on>2013-06-13T21:00:00Z</created_on><updated_on>2013-06-13T21:00:00Z</updated_on></project></projects>";
        string projectsXmlCount3Offset1Limit1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"3\" offset=\"1\" limit=\"1\" type=\"array\"><project><id>2</id><name>Redmine Task List</name><identifier>redminetasklist</identifier><description></description><created_on>2013-06-13T21:00:00Z</created_on><updated_on>2013-06-13T21:00:00Z</updated_on></project></projects>";
        string projectsXmlCount3Offset2Limit1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"3\" offset=\"2\" limit=\"1\" type=\"array\"><project><id>3</id><name>Redmine Task List</name><identifier>redminetasklist</identifier><description></description><created_on>2013-06-13T21:00:00Z</created_on><updated_on>2013-06-13T21:00:00Z</updated_on></project></projects>";
    }
}

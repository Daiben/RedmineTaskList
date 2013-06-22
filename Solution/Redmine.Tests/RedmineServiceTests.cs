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
        [SetUp]
        public void SetUp()
        {
            RedmineService.ClearUserCache();
        }

        [Test]
        public void GetIssues()
        {
            var request = CreateRequestMock(usersXml, issuesXml);
            
            var issues = RedmineService.GetIssues("lemon", "Pa$sw0rd", "test://redmine/");

            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual("Parse Redmine API XML", issues[0].Subject);
        }

        [Test]
        public void GetIssues_AssertUri()
        {
            var request = CreateRequestMock(usersXml, issuesXml);
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/issues.xml?assigned_to_id=1"))).Repeat.Once();
            
            RedmineService.GetIssues("lemon", "Pa$sw0rd", "test://redmine/");

            request.VerifyAllExpectations();
        }

        [Test]
        public void GetIssues_AssertUserIdIsCached()
        {
            var request = CreateRequestMock(usersXml, issuesXml, issuesXml);
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml"))).Repeat.Once();

            RedmineService.GetIssues("lemon", "Pa$sw0rd", "test://redmine/");
            RedmineService.GetIssues("lemon", "Pa$sw0rd", "test://redmine/");

            request.VerifyAllExpectations();
        }

        [Test]
        public void GetIssues_AssertUserIdIsCachedWithRespectToUrl()
        {
            var request = CreateRequestMock(usersXml, issuesXml, usersXml, issuesXml);
            request.Expect(x => x.Create(new Uri("test://redmine1/users.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine2/users.xml"))).Repeat.Once();
            
            RedmineService.GetIssues("lemon", "Pa$sw0rd", "test://redmine1/");
            RedmineService.GetIssues("lemon", "Pa$sw0rd", "test://redmine2/");

            request.VerifyAllExpectations();
        }

        [Test]
        public void GetIssues_AssertUsersAreRequestedContinuouslyUntilLoginFound()
        {
            var request = CreateRequestMock(usersXmlCount3Offset0Limit1, usersXmlCount3Offset1Limit1, issuesXml);
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml?offset=1"))).Repeat.Once();
            
            RedmineService.GetIssues("lemon", "Pa$sw0rd", "test://redmine/");

            request.VerifyAllExpectations();
        }

        [Test]
        public void GetIssues_AssertCustomQuery()
        {
            var request = CreateRequestMock(usersXml, issuesXml);
            request.Expect(x => x.Create(new Uri("test://redmine/issues.xml?assigned_to_id=1&limit=3"))).Repeat.Once();
            
            RedmineService.GetIssues("lemon", "Pa$sw0rd", "test://redmine/", "assigned_to_id={0}&limit=3");

            request.VerifyAllExpectations();
        }


        [Test]
        public void GetProjects()
        {
            var request = CreateRequestMock(usersXml, projectsXml);
            
            var projects = RedmineService.GetProjects("lemon", "Pa$sw0rd", "test://redmine/");

            Assert.AreEqual(1, projects.Length);
            Assert.AreEqual("Redmine Task List", projects[0].Name);
        }

        [Test]
        public void GetProjects_AssertUri()
        {
            var request = CreateRequestMock(usersXml, projectsXml);
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/projects.xml"))).Repeat.Once();
            
            RedmineService.GetProjects("lemon", "Pa$sw0rd", "test://redmine/");

            request.VerifyAllExpectations();
        }
        
        [Test]
        public void GetProjects_AssertAllAreRequested()
        {
            var request = CreateRequestMock(usersXml, projectsXmlCount3Offset0Limit1, projectsXmlCount3Offset1Limit1, projectsXmlCount3Offset2Limit1);
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/projects.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/projects.xml?offset=1"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/projects.xml?offset=2"))).Repeat.Once();
            
            var projects = RedmineService.GetProjects("lemon", "Pa$sw0rd", "test://redmine/");

            Assert.AreEqual(3, projects.Length);
            request.VerifyAllExpectations();
        }

        [Test]
        public void GetProjects_AssertUserIdIsCached()
        {
            var request = CreateRequestMock(usersXml, projectsXml, projectsXml);
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml"))).Repeat.Once();

            RedmineService.GetProjects("lemon", "Pa$sw0rd", "test://redmine/");
            RedmineService.GetProjects("lemon", "Pa$sw0rd", "test://redmine/");

            request.VerifyAllExpectations();
        }

        [Test]
        public void GetProjects_AssertUserIdIsCachedWithRespectToUrl()
        {
            var request = CreateRequestMock(usersXml, projectsXml, usersXml, projectsXml);
            request.Expect(x => x.Create(new Uri("test://redmine1/users.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine2/users.xml"))).Repeat.Once();
            
            RedmineService.GetProjects("lemon", "Pa$sw0rd", "test://redmine1/");
            RedmineService.GetProjects("lemon", "Pa$sw0rd", "test://redmine2/");

            request.VerifyAllExpectations();
        }
        
        [Test]
        public void GetProjects_AssertUsersAreRequestedContinuouslyUntilLoginFound()
        {
            var request = CreateRequestMock(usersXmlCount3Offset0Limit1, usersXmlCount3Offset1Limit1, projectsXml);
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml"))).Repeat.Once();
            request.Expect(x => x.Create(new Uri("test://redmine/users.xml?offset=1"))).Repeat.Once();
            
            RedmineService.GetProjects("lemon", "Pa$sw0rd", "test://redmine/");

            request.VerifyAllExpectations();
        }


        private static TestWebRequest CreateRequestMock(params string[] responseXml)
        {
            var request = MockRepository.GenerateMock<TestWebRequest>();
            var response = CreateResponseStub(responseXml);
            
            request.Expect(x => x.Headers.Add("", "")).IgnoreArguments();
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

        
        string usersXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><user><id>1</id><login>lemon</login><firstname>Dmitry</firstname><lastname>Popov</lastname><mail>lemon@yandex.ru</mail><created_on>2013-06-13T21:30:03Z</created_on><last_login_on>2013-06-13T22:15:54Z</last_login_on></user></users>";
        string issuesXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><issues total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><issue><id>1</id><project id=\"2\" name=\"Redmine API Library\"/><tracker id=\"2\" name=\"Feature\"/><status id=\"3\" name=\"Resolved\"/><priority id=\"2\" name=\"Normal\"/><author id=\"1\" name=\"Dmitry Popov\"/><assigned_to id=\"1\" name=\"Dmitry Popov\"/><subject>Parse Redmine API XML</subject><description>Users, projects and issues</description><start_date>2013-06-13</start_date><due_date>2013-06-14</due_date><done_ratio>100</done_ratio><estimated_hours>2</estimated_hours><created_on>2013-06-13T22:10:24Z</created_on><updated_on>2013-06-13T22:10:24Z</updated_on><closed_on>2013-06-14T00:15:03Z</closed_on></issue></issues>";
        string projectsXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><project><id>1</id><name>Redmine Task List</name><identifier>redminetasklist</identifier><description></description><created_on>2013-06-13T21:00:00Z</created_on><updated_on>2013-06-13T21:00:00Z</updated_on></project></projects>";
        
        string usersXmlCount3Offset0Limit1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users total_count=\"3\" offset=\"0\" limit=\"1\" type=\"array\"><user><id>1</id><login>admin</login><firstname>Dmitry</firstname><lastname>Popov</lastname><mail>lemon@yandex.ru</mail><created_on>2013-06-13T21:30:03Z</created_on><last_login_on>2013-06-13T22:15:54Z</last_login_on></user></users>";
        string usersXmlCount3Offset1Limit1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users total_count=\"3\" offset=\"1\" limit=\"1\" type=\"array\"><user><id>2</id><login>lemon</login><firstname>Dmitry</firstname><lastname>Popov</lastname><mail>lemon@yandex.ru</mail><created_on>2013-06-13T21:30:03Z</created_on><last_login_on>2013-06-13T22:15:54Z</last_login_on></user></users>";

        string projectsXmlCount3Offset0Limit1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"3\" offset=\"0\" limit=\"1\" type=\"array\"><project><id>1</id><name>Redmine Task List</name><identifier>redminetasklist</identifier><description></description><created_on>2013-06-13T21:00:00Z</created_on><updated_on>2013-06-13T21:00:00Z</updated_on></project></projects>";
        string projectsXmlCount3Offset1Limit1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"3\" offset=\"1\" limit=\"1\" type=\"array\"><project><id>2</id><name>Redmine Task List</name><identifier>redminetasklist</identifier><description></description><created_on>2013-06-13T21:00:00Z</created_on><updated_on>2013-06-13T21:00:00Z</updated_on></project></projects>";
        string projectsXmlCount3Offset2Limit1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projects total_count=\"3\" offset=\"2\" limit=\"1\" type=\"array\"><project><id>3</id><name>Redmine Task List</name><identifier>redminetasklist</identifier><description></description><created_on>2013-06-13T21:00:00Z</created_on><updated_on>2013-06-13T21:00:00Z</updated_on></project></projects>";
    }
}

using System;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace Redmine.Tests
{
    [TestFixture]
    public class RedmineWebRequestTests
    {
        [Test]
        public void GetResponse()
        {
            var uri = new Uri("test://redmine.org/users.xml");
            var request = MockRepository.GenerateStrictMock<TestWebRequest>();
            var response = MockRepository.GenerateStub<WebResponse>();
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><user><id>1</id><login>lemon</login><firstname>Dmitry</firstname><lastname>Popov</lastname><mail>lemon@yandex.ru</mail><created_on>2013-06-13T21:30:03Z</created_on><last_login_on>2013-06-13T22:15:54Z</last_login_on></user></users>";
            var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            
            WebRequest.RegisterPrefix("test", TestWebRequest.GetCreator(request));
            
            request.Expect(x => x.Create(uri));
            request.Expect(x => x.Headers.Add("Authorization", "Basic bGVtb246UGEkc3cwcmQ="));
            request.Expect(x => x.GetResponse()).Return(response);
            response.Stub(x => x.GetResponseStream()).Return(responseStream);

            var redmineRequest = new RedmineWebRequest("lemon", "Pa$sw0rd", uri);
            var result = redmineRequest.GetResponse();

            request.VerifyAllExpectations();
            Assert.AreEqual(xml, result);
        }
    }
}

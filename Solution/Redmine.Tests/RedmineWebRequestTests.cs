using System;
using System.IO;
using System.Net;
using System.Net.Security;
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
            var request = CreateRequestMock();
            
            request.Expect(x => x.GetResponse()).Return(CreateResponseStub());
            request.Expect(x => x.Create(uri));
            request.Expect(x => x.Credentials = null).IgnoreArguments(); // TODO: check credentials

            var redmineRequest = new RedmineWebRequest("lemon", "Pa$sw0rd", uri);
            var result = redmineRequest.GetResponse();

            request.VerifyAllExpectations();
            Assert.AreEqual(xml, result);
        }

        [Test]
        public void GetResponse_CertificateValidationCallbackIsUsed()
        {
            var request = CreateRequestMock();
            var callbackUsedForRequest = default(RemoteCertificateValidationCallback);
            request.Expect(x => x.GetResponse()).Do((Func<WebResponse>)(() => 
            {
                callbackUsedForRequest = ServicePointManager.ServerCertificateValidationCallback;
                
                return CreateResponseStub();
            }));

            var callbackUsedBeforeRequest = ServicePointManager.ServerCertificateValidationCallback;
            new RedmineWebRequest("lemon", "Pa$sw0rd", uri).GetResponse();
            var callbackUsedAfterRequest = ServicePointManager.ServerCertificateValidationCallback;

            Assert.IsNotNull(callbackUsedForRequest);            
            Assert.AreEqual(typeof(CertificateValidator), callbackUsedForRequest.Method.DeclaringType);
            Assert.AreEqual("ValidateCertificate", callbackUsedForRequest.Method.Name);
            Assert.AreSame(callbackUsedBeforeRequest, callbackUsedAfterRequest);
        }

        [Test]
        public void GetResponse_CustomAuthenticationIsUsed()
        {
            var request = CreateRequestMock();
            var authenticationUsedForRequest = default(IAuthenticationModule);
            request.Expect(x => x.GetResponse()).Do((Func<WebResponse>)(() => 
            {
                authenticationUsedForRequest = GetBasicAuthenticationModule();
                return CreateResponseStub();
            }));

            var authenticationUsedBeforeRequest = GetBasicAuthenticationModule();
            new RedmineWebRequest("lemon", "Pa$sw0rd", uri).GetResponse();
            var authenticationUsedAfterRequest = GetBasicAuthenticationModule();

            Assert.IsNotNull(authenticationUsedForRequest);            
            Assert.IsInstanceOf<BasicClient>(authenticationUsedForRequest);
            Assert.AreSame(authenticationUsedBeforeRequest, authenticationUsedAfterRequest);
        }

        private static IAuthenticationModule GetBasicAuthenticationModule()
        {
            IAuthenticationModule basicModule = null;
            
            var registeredModulesEnumerator = AuthenticationManager.RegisteredModules;

            while (registeredModulesEnumerator.MoveNext())
            {
                var module = registeredModulesEnumerator.Current as IAuthenticationModule;
            
                if (module.AuthenticationType.Equals("Basic"))
                {
                    basicModule = module;
                    break;
                }
            }

            return basicModule;
        }
        

        private TestWebRequest CreateRequestMock()
        {
            var request = MockRepository.GenerateMock<TestWebRequest>();
            
            WebRequest.RegisterPrefix("test", TestWebRequest.GetCreator(request));
         
            return request;
        }

        private WebResponse CreateResponseStub()
        {
            var response = MockRepository.GenerateStub<WebResponse>();
            var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            
            response.Stub(x => x.GetResponseStream()).Return(responseStream);
            
            return response;
        }

        Uri uri = new Uri("test://redmine.org/users.xml");
        string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users total_count=\"1\" offset=\"0\" limit=\"25\" type=\"array\"><user><id>1</id><login>lemon</login><firstname>Dmitry</firstname><lastname>Popov</lastname><mail>lemon@yandex.ru</mail><created_on>2013-06-13T21:30:03Z</created_on><last_login_on>2013-06-13T22:15:54Z</last_login_on></user></users>";
    }
}

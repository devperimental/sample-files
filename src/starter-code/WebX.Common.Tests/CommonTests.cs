using Moq;
using Moq.AutoMock;
using NUnit.Framework.Legacy;
using PlatformX.Http.Behaviours;

namespace WebX.Common.Tests
{
    public class CommonTests
    {
        private AutoMocker _mocker;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
        }


        [Test]
        public void Test1()
        {
            var httpContextHelper = _mocker.GetMock<IHttpContextHelper>();
            httpContextHelper.Setup(c => c.DetermineIpAddress()).Returns("10.0.0.1");
            httpContextHelper.Setup(c => c.DetermineUserAgent()).Returns("UserAgent");
            httpContextHelper.Setup(c => c.GetHeaderKeyValue(It.IsAny<string>())).Returns(string.Empty);
            var requestHelper = _mocker.CreateInstance<ClientRequestHelper>();

            var parameters = new Dictionary<string, string> { { "CONTAINERNAME", "CONTAINERNAME" } };
            var response = requestHelper.BuildClientRequest(parameters);

            ClassicAssert.IsNotNull(response);
        }
    }
}
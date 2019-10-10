using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LoadTestTools.SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        [HttpGet("{waitMilliseconds}", Name = "Get")]
        public async Task<SampleResponse> Get(int waitMilliseconds = 50)
        {
            await Task.Delay(waitMilliseconds);

            return new SampleResponse
            {
                WaitMilliseconds = waitMilliseconds
            };
        }

        [HttpPost("", Name = "Post")]
        public async Task<SampleResponse> Post(SamplePayload samplePayload)
        {
            await Task.Delay(samplePayload.WaitMilliseconds);

            return new SampleResponse
            {
                WaitMilliseconds = samplePayload.WaitMilliseconds
            };
        }
    }


}

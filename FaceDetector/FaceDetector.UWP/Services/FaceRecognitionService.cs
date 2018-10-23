using Microsoft.ProjectOxford.Face;
using MotionDetector.UWP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDetector.UWP.Services
{
    public class FaceRecognitionService
    {
        public FaceRecognitionService()
        {
            FaceServiceClient = new FaceServiceClient("<<API key here>>", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");
        }

        public FaceServiceClient FaceServiceClient { get; }

        public async Task<string> VerifyFaceAgainstTraindedGroup(string personGroupId, Stream stream)
        {
            try
            {
                var faces = await FaceServiceClient.DetectAsync(stream);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                var results = await FaceServiceClient.IdentifyAsync(personGroupId, faceIds);
                foreach (var identifyResult in results)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Length == 0)
                    {
                        return "No one identified";
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await FaceServiceClient.GetPersonAsync(personGroupId, candidateId);
                        return "Identified as: " + person.Name;
                    }
                }
                return "No one identified";
            }

            catch(FaceAPIException ex)
            {
                return "An error has occurred: " + ex.Message;
            }
        }
    }
}

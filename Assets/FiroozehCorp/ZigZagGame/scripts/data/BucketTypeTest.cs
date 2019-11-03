using Newtonsoft.Json;

namespace FiroozehCorp.ZigZagGame.scripts.data
{
    [System.Serializable]
    public class BucketTypeTest
    {
        
        [JsonProperty("name")]
        public string Name { set; get; }
        
        [JsonProperty("age")]
        public int Age{ set; get; }


        public override string ToString()
        {
            return "Name : " + Name +", Age : "+ Age + '\n' ;
        }
    }
}
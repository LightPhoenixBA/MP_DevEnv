using LightPhoenixBA.StrideExtentions.MultiplayerBase;
using Stride.Engine;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MultiplayerServer.Sample;

public class PlayerSampleScript : AsyncScript
{
	 public StrideClientBase strideClient { get; private set; }

	 public override async Task Execute()
	 {
			strideClient = new();
			//await Script.NextFrame();
			await strideClient.Execute();
	 }
}

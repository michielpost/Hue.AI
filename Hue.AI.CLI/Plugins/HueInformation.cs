using HueApi;
using HueApi.ColorConverters.Original.Extensions;
using HueApi.Models;
using HueApi.Models.Requests;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Hue.AI.CLI.Plugins
{
    public class HueInformation(LocalHueApi api)
    {
        [KernelFunction]
        [Description("Get all available lights. Each light has a device id. Use it to find the correct light id based on a device id you got from a room.")]
        public async Task<List<Light>> GetLights(Kernel kernel)
        {
            var result = await api.GetLightsAsync();
            return result.Data;
        }

        [KernelFunction]
        [Description("Get all available rooms which includes ids of devices in the room.")]
        public async Task<List<Room>> GetRooms(Kernel kernel)
        {
            var result = await api.GetRoomsAsync();
            return result.Data;
        }

        //[KernelFunction]
        //[Description("Get all available light scenes the system can set.")]
        //public async Task<List<Scene>> GetScenes(Kernel kernel)
        //{
        //    var result = await api.GetScenesAsync();
        //    return result.Data;
        //}

        [KernelFunction]
        [Description("Change the light to a certain color, supply a HEX color value")]
        public async Task<string> UpdatLightColor(Kernel kernel, [Description("Resource Id of a light. Always a Guid. This must be a light Id, never a device id.")] Guid lightId, string hexColor)
        {
            Console.WriteLine($"Setting lightId: {lightId} to color: {hexColor}");

            var req = new UpdateLight()
                .TurnOn()
                .SetColor(new HueApi.ColorConverters.RGBColor(hexColor));

            var result = await api.UpdateLightAsync(lightId, req);

            return $"lightId: {lightId} now has color {hexColor}";
        }


        [KernelFunction]
        [Description("Can turn a single light off")]
        public async Task<string> TurnOffLight(Kernel kernel, [Description("Resource Id of a light. Always a Guid. This must be a light Id, never a device id.")] Guid lightId)
        {
            Console.WriteLine($"Turned Off lightId: {lightId}");

            var req = new UpdateLight()
                .TurnOff();

            var result = await api.UpdateLightAsync(lightId, req);

            return $"Turned Off lightId: {lightId}";
        }

        [KernelFunction]
        [Description("Can turn all light off at the same time")]
        public async Task<string> TurnOffAllLights(Kernel kernel)
        {
            Console.WriteLine($"Turned Off all lights");

            var all = await api.GetGroupedLightsAsync();
            var groupId = all.Data.Where(x => x.IdV1 == "/groups/0").First().Id; //All

            var req = new UpdateGroupedLight()
                .TurnOff();

            var result = await api.UpdateGroupedLightAsync(groupId, req);

            return "Turned Off all lights";
        }

    }
}

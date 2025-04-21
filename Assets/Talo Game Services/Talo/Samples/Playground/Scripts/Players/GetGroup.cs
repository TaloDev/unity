using System;
using UnityEngine;

namespace TaloGameServices.Sample.Playground
{
    public class GetGroup : MonoBehaviour
    {
        public string groupId;

        public async void OnButtonClick()
        {
            if (string.IsNullOrEmpty(groupId))
            {
                ResponseMessage.SetText("groupId not set on GetGroupButton");
                return;
            }

            try
            {
                var group = await Talo.PlayerGroups.Get(groupId);
                ResponseMessage.SetText($"{group.name} has {group.count} player(s)");
            }
            catch (Exception e)
            {
                ResponseMessage.SetText(e.Message);
            }
        }
    }
}

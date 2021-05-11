using System.Collections.Generic;
using Mirror.Cloud.ListServerService;
using UnityEngine;

namespace Mirror.Cloud.Example
{
    /// <summary>
    ///     Displays the list of servers
    /// </summary>
    public class ServerListUI : MonoBehaviour
    {
        private readonly List<ServerListUIItem> items = new List<ServerListUIItem>();
        [SerializeField] private readonly ServerListUIItem itemPrefab = null;
        [SerializeField] private Transform parent;

        private void OnValidate()
        {
            if (parent == null) parent = transform;
        }

        public void UpdateList(ServerCollectionJson serverCollection)
        {
            DeleteOldItems();
            CreateNewItems(serverCollection.servers);
        }

        private void CreateNewItems(ServerJson[] servers)
        {
            foreach (var server in servers)
            {
                var clone = Instantiate(itemPrefab, parent);
                clone.Setup(server);
                items.Add(clone);
            }
        }

        private void DeleteOldItems()
        {
            foreach (var item in items) Destroy(item.gameObject);

            items.Clear();
        }
    }
}
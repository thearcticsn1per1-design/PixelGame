using UnityEngine;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// Manages dungeon floor generation and progression
    /// </summary>
    public class FloorManager : MonoBehaviour
    {
        [Header("Floor Settings")]
        [SerializeField] private int roomsPerFloor = 10;
        [SerializeField] private float roomSpacing = 30f;

        [Header("Room Prefabs")]
        [SerializeField] private List<GameObject> combatRoomPrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> bossRoomPrefabs = new List<GameObject>();
        [SerializeField] private GameObject shopRoomPrefab;
        [SerializeField] private GameObject treasureRoomPrefab;
        [SerializeField] private GameObject restRoomPrefab;

        [Header("Generation Settings")]
        [SerializeField] private int shopRoomFrequency = 4; // Every N rooms
        [SerializeField] private int treasureRoomFrequency = 3;

        // State
        public int CurrentFloor { get; private set; } = 1;
        private List<Room> generatedRooms = new List<Room>();
        private int currentRoomIndex = 0;
        private GameObject floorContainer;

        public void GenerateFloor(int floorNumber)
        {
            CurrentFloor = floorNumber;

            // Clean up previous floor
            CleanupFloor();

            // Create container
            floorContainer = new GameObject($"Floor_{floorNumber}");

            // Generate rooms
            for (int i = 0; i < roomsPerFloor; i++)
            {
                Room.RoomType roomType = DetermineRoomType(i);
                GameObject roomPrefab = GetRoomPrefab(roomType);

                if (roomPrefab != null)
                {
                    SpawnRoom(roomPrefab, i);
                }
            }

            GameEvents.FloorStarted(floorNumber);
        }

        private Room.RoomType DetermineRoomType(int roomIndex)
        {
            // Last room is always boss
            if (roomIndex == roomsPerFloor - 1)
            {
                return Room.RoomType.Boss;
            }

            // Shop rooms
            if (shopRoomFrequency > 0 && (roomIndex + 1) % shopRoomFrequency == 0)
            {
                return Room.RoomType.Shop;
            }

            // Treasure rooms
            if (treasureRoomFrequency > 0 && (roomIndex + 1) % treasureRoomFrequency == 0)
            {
                return Room.RoomType.Treasure;
            }

            // Random special room chance
            float specialChance = Random.value;
            if (specialChance < 0.05f)
            {
                return Room.RoomType.Rest;
            }

            // Default to combat room
            return Room.RoomType.Combat;
        }

        private GameObject GetRoomPrefab(Room.RoomType roomType)
        {
            switch (roomType)
            {
                case Room.RoomType.Combat:
                    return combatRoomPrefabs.Count > 0
                        ? combatRoomPrefabs[Random.Range(0, combatRoomPrefabs.Count)]
                        : null;

                case Room.RoomType.Boss:
                    return bossRoomPrefabs.Count > 0
                        ? bossRoomPrefabs[Random.Range(0, bossRoomPrefabs.Count)]
                        : null;

                case Room.RoomType.Shop:
                    return shopRoomPrefab;

                case Room.RoomType.Treasure:
                    return treasureRoomPrefab;

                case Room.RoomType.Rest:
                    return restRoomPrefab;

                default:
                    return null;
            }
        }

        private void SpawnRoom(GameObject roomPrefab, int index)
        {
            // Simple linear layout (can be improved with procedural generation)
            Vector3 position = new Vector3(index * roomSpacing, 0, 0);

            GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity, floorContainer.transform);
            Room room = roomObj.GetComponent<Room>();

            if (room != null)
            {
                room.roomDifficulty = CalculateRoomDifficulty(index);
                generatedRooms.Add(room);
            }
        }

        private int CalculateRoomDifficulty(int roomIndex)
        {
            // Difficulty scales with floor number and room position
            int baseDifficulty = CurrentFloor;
            int roomProgression = roomIndex / 3; // Increase every 3 rooms

            return baseDifficulty + roomProgression;
        }

        public void OnRoomComplete()
        {
            currentRoomIndex++;

            if (currentRoomIndex >= roomsPerFloor)
            {
                OnFloorComplete();
            }
        }

        private void OnFloorComplete()
        {
            GameEvents.FloorCompleted(CurrentFloor);

            // Generate next floor
            GenerateFloor(CurrentFloor + 1);
        }

        public void CleanupFloor()
        {
            // Clean up all rooms
            foreach (var room in generatedRooms)
            {
                if (room != null)
                {
                    room.Cleanup();
                }
            }

            generatedRooms.Clear();
            currentRoomIndex = 0;

            // Destroy floor container
            if (floorContainer != null)
            {
                Destroy(floorContainer);
            }
        }

        public Room GetCurrentRoom()
        {
            if (currentRoomIndex >= 0 && currentRoomIndex < generatedRooms.Count)
            {
                return generatedRooms[currentRoomIndex];
            }
            return null;
        }
    }
}

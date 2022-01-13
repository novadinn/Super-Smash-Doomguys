using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour {
    [SerializeField] Player[] players_;

    [System.Serializable]
    class TilemapSpawner {
        [SerializeField] Pickup[] possible_pickups_;
        [SerializeField] float timer_check_time_;
        [SerializeField] float timer_speed_;
        Dictionary<Vector2, Pickup> current_pickups_ = new Dictionary<Vector2, Pickup>();
        Timer pickup_check_timer_ = null;
        float pickup_spawn_percent_ = 0.0f;

        static List<Vector2> kActiveTiles;

        public void init(List<Vector2> all_tiles_positions) {
            kActiveTiles = new List<Vector2>(all_tiles_positions);

            pickup_check_timer_ = new Timer(timer_check_time_);
        }

        public void update(float elapsed_time) {
            pickup_spawn_percent_ += elapsed_time * timer_speed_;
            pickup_check_timer_.update(Time.deltaTime);

            if(!pickup_check_timer_.active()) {
                pickup_check_timer_.reset();
                if((UnityEngine.Random.value * 100) < pickup_spawn_percent_ && kActiveTiles.Count > 0) {
                    pickup_spawn_percent_ = 0.0f;
                    int position_index = Random.Range(0, kActiveTiles.Count);
                    current_pickups_.Add(
                        kActiveTiles[position_index], 
                        GameObject.Instantiate(possible_pickups_[Random.Range(0, possible_pickups_.Length)], kActiveTiles[position_index] + new Vector2(0,1), Quaternion.identity)
                        );
                    kActiveTiles.RemoveAt(position_index);
                }
            }

            foreach(KeyValuePair<Vector2, Pickup> pickup_position in current_pickups_) {
                if(!pickup_position.Value) {
                    kActiveTiles.Add(pickup_position.Key);
                    current_pickups_.Remove(pickup_position.Key);
                    break;
                }
            }
        }
    }
    [SerializeField] TilemapSpawner[] spawners_;

    void Awake() {
        Tilemap tilemap = FindObjectOfType<Tilemap>();

        List<Vector2> all_tiles_positions = new List<Vector2>();
 
        for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; ++x) {
            for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++) {
                Vector3Int tile_local_position = (new Vector3Int(x, y, (int)tilemap.transform.position.y));
                Vector3 tile_world_position = tilemap.CellToWorld(tile_local_position);
                if (tilemap.HasTile(tile_local_position)) {
                    if(!tilemap.HasTile(tile_local_position + new Vector3Int(0, 1, 0)) && !tilemap.HasTile(tile_local_position + new Vector3Int(0, 2, 0))) {
                        all_tiles_positions.Add(new Vector3(tile_world_position.x + 0.5f, tile_world_position.y + 1.0f));
                    }
                }
            }
        }

        foreach(TilemapSpawner spawner in spawners_) {
            spawner.init(all_tiles_positions);
        }

        List<Vector2> used_tiles = new List<Vector2>(all_tiles_positions);
        for(int i = 0; i < players_.Length; ++i) {
            int index = UnityEngine.Random.Range(0, used_tiles.Count);
            players_[i] = Instantiate(players_[i], used_tiles[index] + new Vector2(0,1), Quaternion.identity);
            used_tiles.RemoveAt(index);
        }
    }

    void Update() {
        foreach(TilemapSpawner spawner in spawners_) {
            spawner.update(Time.deltaTime);
        }
    }
}

public enum Team {
	TEAM_A,
	TEAM_B
}
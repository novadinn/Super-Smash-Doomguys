using UnityEngine;

public class Projectile : MonoBehaviour {

    const float kDestroyTime = 10;

    const float kExplodeRadius = 2;

    [SerializeField] GameObject hit_effect_;

    [SerializeField] float damage_;
    [SerializeField] float move_speed_;
    [SerializeField] float push_force_;

    LayerMask collision_layer_;

    void Start() {
        Destroy(gameObject, kDestroyTime);
    }

    public Projectile init(LayerMask layer) {
        collision_layer_ = layer;
        return this;
    }
    
    void Update() {
        float move_distance = move_speed_ * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, move_distance, collision_layer_);
        if(hit) {
            transform.position = hit.point;
            Player player = hit.collider.GetComponent<Player>();
            if(player) {
                player.takeDamage(damage_, hit.point, transform.right, push_force_);
            }
            destroy();
        } else {
            transform.Translate((Vector2)transform.right * move_distance, Space.World);
        }
    }

    public void explode() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, kExplodeRadius, Vector2.zero, 1, collision_layer_);
        foreach(RaycastHit2D hit in hits) {
            Player player = hit.collider.GetComponent<Player>();
            if(player) {
                player.takeDamage(damage_, hit.point, transform.right, push_force_);
            }
        }
        destroy();
    }

    void destroy() {
        Destroy(Instantiate(hit_effect_, transform.position, transform.rotation), 0.417f);
        Destroy(gameObject);
    }
}

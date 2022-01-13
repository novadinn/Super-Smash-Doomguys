using System.Collections;
using UnityEngine;

public abstract class Pickup : MonoBehaviour {
    
    protected const float kBoxSize = 1;

    [SerializeField] GameObject spawn_effect_;

    bool was_init_ = false;

    void Awake() {
        StartCoroutine("spawn");
    }

    protected virtual void Update() {
        if(!was_init_) 
            return;

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * kBoxSize, 0, Vector2.zero, 0, CommonIncludes.players_masks);
        if(hit) {
            reactToCollision(hit);
        }
    }

    protected abstract void reactToCollision(RaycastHit2D hit);

    IEnumerator spawn() {
        GetComponent<SpriteRenderer>().enabled = false;
        GameObject effect = Instantiate(spawn_effect_, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.417f);
        Destroy(effect);
        GetComponent<SpriteRenderer>().enabled = true;
		was_init_ = true;
    }
}
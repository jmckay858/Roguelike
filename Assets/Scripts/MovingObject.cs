using UnityEngine;
using System.Collections;


// abstract class enables me to create a class and class members that are incomplete and must be implemented in a derived class.
public abstract class MovingObject : MonoBehaviour {
	public float moveTime = 0.1f;
	public LayerMask blockingLayer;

	private BoxCollider2D boxCollider;
	private Rigidbody2D rb2D;
	private float inverseMoveTime;

	// Protected virtual functions can be overridden by the inheriting class
	protected virtual void Start () {
		//Get the reference to the collider
		boxCollider = GetComponent<BoxCollider2D> ();

		// Get reference to the objects rigidbody2D
		rb2D = GetComponent<Rigidbody2D>();

		//Store the reciprial of the move time so we can use it by multiplying instead of dividing, this is more efficient
		inverseMoveTime = 1f / moveTime;
	}

	//Move returns trie if it is able to move and false otherwise
	//Move takes a parameter for x direction, y direction and a RaycastHit2D
	protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
	{
		//Start position to move from, based on objects current transform position
		Vector2 start = transform.position;

		//Calculate end position based on the direction parameters passd in when calling move.
		Vector2 end = start + new Vector2 (xDir, yDir);

		//Disable the boxCollider so that linecast does't hit this objects own collider.
		boxCollider.enabled = false;

		//Case a line from start point to end pointchecking collision on blockingLayer.
		hit = Physics2D.Linecast (start, end, blockingLayer);

		//re-Enable boxCollider after linecast
		boxCollider.enabled = true;

		//Check if anthing was hit
		if (hit.transform == null) {
			StartCoroutine (SmoothMovement (end));

			return true;
		}

		return false;

	}

	protected IEnumerator SmoothMovement (Vector3 end)
	{
		//Calculate the remaining distance to move based on the square magnitude of the difference between current positon and end parameter
		//Square magnitude is used instead of magnitude becase it's computationally cheaper
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		//While that distance is greater than a very small amount(Epsilon, almost zero):
		while (sqrRemainingDistance > float.Epsilon) {
			//Find a new position proportionally closer to the end, based on the moveTime
			Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

			//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
			rb2D.MovePosition (newPosition);

			//Recalculate the remaining distance after moving
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;

			//Return and loop until sqrRemainingDistance is close enough to zero to end the function
			yield return null;
		}

	}

	//The virtual keyword means AttemptMove can be overridden by inheriting classes using the override keywork.
	//AttemptMove takes a generic parameter T to specify the type of component we expect out unit to interact with if blocked(Player for Enemies, Wall for Player);
	protected virtual void AttemptMove <T> (int xDir, int yDir) 
		where T : Component
	{
		//Hit will sote whatever our linecast hits when MOve is called.
		RaycastHit2D hit;

		//Set canMove to true if Move was successful, false if failed/
		bool canMove = Move (xDir, yDir, out hit);

		//Check if nothing was hit by linecast
		if (hit.transform == null)
			//If nothing was hit, return and don't execute further code
			return;

		//Get a component reference to the component of type T attached to the object that was hit
		T hitComponent = hit.transform.GetComponent <T> ();

		//If carMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with
		if (!canMove && hitComponent != null)

			//Call the OnCanMove function and pass it hitComponent as parameter.
			OnCantMove (hitComponent);
	}

	//The abstract modifier indicates that the thing being modified has a missing or incomplete implementation
	//OnCantMove will be overridden by functions in the inheriting classes.
	protected abstract void OnCantMove <T> (T component)
		where T : Component;


	
}

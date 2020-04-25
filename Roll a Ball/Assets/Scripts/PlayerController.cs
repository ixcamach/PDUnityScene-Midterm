using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour{

    public float speed;
    public Text countText;
    public Text winText;

    private Rigidbody rb;
    private int count;

    void Start ()
    {
      Application.runInBackground = true; //allows unity to update when not in focus

      //************* Instantiate the OSC Handler...
      OSCHandler.Instance.Init ();
      OSCHandler.Instance.SendMessageToClient("pd", "/unity/playseq", 1);
      //*************

        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText ();
        winText.text = "";
    }

    void FixedUpdate ()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        rb.AddForce(movement * speed);
        //************* Routine for receiving the OSC...
    		OSCHandler.Instance.UpdateLogs();
    		Dictionary<string, ServerLog> servers = new Dictionary<string, ServerLog>();
    		servers = OSCHandler.Instance.Servers;

    		foreach (KeyValuePair<string, ServerLog> item in servers) {
    			// If we have received at least one packet,
    			// show the last received from the log in the Debug console
    			if (item.Value.log.Count > 0) {
    				int lastPacketIndex = item.Value.packets.Count - 1;

    				//get address and data packet
    				countText.text = item.Value.packets [lastPacketIndex].Address.ToString ();
    				countText.text += item.Value.packets [lastPacketIndex].Data [0].ToString ();

    			}
    		}
    		//*************
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up")){
            other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText ();
            if(count < 3){
                OSCHandler.Instance.SendMessageToClient("pd", "/unity/tempo", 500);
            }
            if (count < 6){
                OSCHandler.Instance.SendMessageToClient("pd", "/unity/tempo", 400);
            }
            else if(count < 9){
                OSCHandler.Instance.SendMessageToClient("pd", "/unity/tempo", 300);
            }
            else if (count < 12){
                OSCHandler.Instance.SendMessageToClient("pd", "/unity/tempo", 150);
            }
            else{
                OSCHandler.Instance.SendMessageToClient("pd", "/unity/playseq", 0);
            }
        }
    }

    void OnCollisionEnter(Collision other){
      if (other.gameObject.CompareTag("Wall")){
        Debug.Log("-------- HIT THE WALL ----------");
        OSCHandler.Instance.SendMessageToClient("pd", "/unity/colwall", 1);
      }
    }
    void SetCountText ()
    {
        countText.text = "Count" + count.ToString();
        if (count >= 14)
        {
            winText.text = "You Win!";
        }
        //************* Send the message to the client...
        OSCHandler.Instance.SendMessageToClient ("pd", "/unity/trigger", count);
        //*************
    }
}

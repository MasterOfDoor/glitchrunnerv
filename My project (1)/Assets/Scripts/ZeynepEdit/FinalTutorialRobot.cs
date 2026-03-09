using UnityEngine;
using TMPro;
using System.Collections;

public class FinalTutorialRobot : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform player;
    public Transform escapePoint; 
    public DialogueBubble dialogueBubble; 
    public Animator anim; // MÜFETTİŞ NAZ: Animator'ı buraya ekledik!

    [Header("Hareket Ayarları")]
    public float triggerDistance = 3f;
    public float moveSpeed = 3f;
    public float runSpeed = 8f;

    [Header("Diyalog")]
    [TextArea]
    public string[] dialogueLines;

    private bool isTalking = false;
    private bool isEscaping = false;

    void Start()
    {
        // Eğer Inspector'dan sürüklemezsen kod kendisi bulmaya çalışsın
        if (anim == null) anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        // ANİMASYON KONTROLÜ: Robot konuşmuyorsa ve bir yere gidiyorsa yürüme animasyonu çalışsın
        if (anim != null)
        {
            // Eğer isTalking false ise ve robot hareket ediyorsa "isWalking" true olur
            bool moving = !isTalking; 
            anim.SetBool("isWalking", moving); 
        }

        if (isTalking) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (!isEscaping && dist > triggerDistance)
        {
            MoveTo(player.position, moveSpeed);
        }
        else if (!isEscaping && dist <= triggerDistance)
        {
            StartCoroutine(StartFullDialogue());
        }

        if (isEscaping)
        {
            MoveTo(escapePoint.position, runSpeed);
            if (Vector3.Distance(transform.position, escapePoint.position) < 0.5f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    void MoveTo(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        
        // 2D oyun olduğu için robotun sağa sola bakmasını sağlayalım
        if (target.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1); // Sağa bak
        else if (target.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1); // Sola bak
    }

    IEnumerator StartFullDialogue()
    {
        isTalking = true;
        
        // Konuşurken durması için animasyonu durduralım
        if (anim != null) anim.SetBool("isWalking", false);

        foreach (string line in dialogueLines)
        {
            dialogueBubble.ShowLine(line);
            // Cümleler arası hızı buradan 1.5f yaparak hızlandırdım:
            float waitTime = (line.Length / 20f) + 0.5f; 
            yield return new WaitForSeconds(waitTime);
        }

        isTalking = false;
        isEscaping = true; 
    }
}
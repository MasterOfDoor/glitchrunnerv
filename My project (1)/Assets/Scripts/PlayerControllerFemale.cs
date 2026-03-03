using UnityEngine;
using System.Collections;

public class PlayerControllerFemale : MonoBehaviour {
    [Header("Hareket Ayarları")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f; 
    public float dashDuration = 0.2f; 
    public float jumpForce = 12f; // Zıplama gücü

    private bool isDashing = false;
    private bool isGrounded; // Yerde olup olmadığını kontrol etmek için

    [Header("Bileşenler")]
    public Rigidbody2D rb;
    public Animator animator;
    Vector2 movement;

    void Update() {
        if (isDashing) return;

        // Hareket Girdileri
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Animator Parametreleri (Büyük harf uyumlu)
        if (movement != Vector2.zero) {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
        }
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // --- TUŞ ATAMALARI ---

        // SPACE: Zıplama (Hoplama)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            Jump();
        }

        // LEFT SHIFT: Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing) {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate() {
        if (isDashing) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        
        // Basit bir yer kontrolü (Kendi sistemine göre güncelleyebilirsin)
        // Karakterin hızı Y ekseninde çok küçükse "yerde" sayıyoruz
        isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;
    }

    void Jump() {
        // Animator'daki "doJump" trigger'ını ateşler
        animator.SetTrigger("doJump"); 
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    IEnumerator Dash() {
        isDashing = true;
        animator.SetTrigger("doDash"); 

        Vector2 dashDir = (movement == Vector2.zero) ? (Vector2)transform.right : movement.normalized;
        rb.linearVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }
}
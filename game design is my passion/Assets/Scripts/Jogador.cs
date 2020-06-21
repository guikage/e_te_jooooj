using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(Controlador2D))]
public class Jogador : MonoBehaviour
{
    public float alturaPulo = 4f;
    public float tempoParaApicePulo = 0.4f;
    public float tempoAceleracaoNoAr = .2f;
	public float tempoAceleracaoNoChao = .1f;
    public float velocidadeMovimento = 20f;

    private float gravidade;
    private float velocidadePulo;
    private Vector2 velocidade;
    private float suavizacaoXVelocidade;

    private Controlador2D controlador;

    public bool estaNoChao;

    public int vidaMaxima = 3;
    public int vidaAtual;
    public int moedas;
    public Direcao direcaoOlhar = Direcao.Direita;
    private Animator animador;
    public bool estaPulando;
    public bool estaAndando;
    public bool estaParado;

    void Awake()
    {
        controlador = GetComponent<Controlador2D>();
        animador = GetComponent<Animator>();

        // Você consegue entender o que estamos fazendo abaixo?
        // Dica: S = S0 + v0.t + (a.t²)/2
        // Dica: v = v0 + at
        gravidade = -(alturaPulo * 2) / Mathf.Pow(tempoParaApicePulo, 2);
        velocidadePulo = Mathf.Abs(gravidade) * tempoParaApicePulo;

        vidaAtual = vidaMaxima;
    }

    // Update is called once per frame
    void Update()
    {
        // Experimente o código e depois remova o comentário do trecho abaixo. Consegue notar o que mudou? *Para remover múltiplas linhas remova o /* e o */.
        if (controlador.colisoes.abaixo || controlador.colisoes.acima)
            velocidade.y = 0f;

        // Armazena o estado do personagem em um atributo dessa classe. Você consegue visualizar ele no Inspector enquanto o jogo roda :D Útil para entender se a sua movimentação está correta.
        estaNoChao = controlador.colisoes.abaixo;

        // Armazena os Inputs verticais do jogador (a, d, seta para esquerda e direita)
        float inputHorizontal = Input.GetAxisRaw("Horizontal");

        // Checa se a seta para cima foi apertada e se o jogador está no chão. Caso tudo seja verdadeiro, modifique nossa velocidade.y para podermos pular.
        // https://docs.microsoft.com/pt-br/dotnet/csharp/language-reference/operators/boolean-logical-operators
        // http://www.inf.ufpr.br/cursos/ci067/Docs/NotasAula/notas-6_Operadores_Logicos.html
        if(Input.GetKeyDown(KeyCode.UpArrow) && controlador.colisoes.abaixo)
        {
            velocidade.y = velocidadePulo;
        }

        if (velocidade.y != 0)
        {
            estaPulando = true;
            estaAndando = false;
            estaParado = false;
        }
        else
        {
            if (inputHorizontal == 0)
            {
                estaAndando = false;
                estaParado = true;
            }
            else
            {
                estaAndando = true;
                estaParado = false;
            }

            estaPulando = false;
        }
        
        animador.SetBool("estaParado", estaParado);
        animador.SetBool("estaAndando", estaAndando);
        animador.SetBool("estaPulando", estaPulando);
            
        // Você consegue entender o que o código abaixo faz?
        // Tente substituír as duas linhas abaixo por a seguinte:
        // velocidade.x = inputHorizontal * velocidadeMovimento;
        // Você nota alguma diferença?
        float velocidadeAlvoX = inputHorizontal * velocidadeMovimento;
		velocidade.x = Mathf.SmoothDamp(velocidade.x, velocidadeAlvoX, ref suavizacaoXVelocidade, controlador.colisoes.abaixo ? tempoAceleracaoNoChao : tempoAceleracaoNoAr);

        // Adiciona nossa gravidade à velocidade.y
        velocidade.y += gravidade * Time.deltaTime;

        // E finalmente move nosso personagem.
        controlador.Mover(velocidade * Time.deltaTime);

        AtualizaOlhar(inputHorizontal);
    }

    public void MudarVida(int valor)
    {
        if (Mathf.Sign(valor) == -1)
        {
            velocidade.x = 30f * (direcaoOlhar == Direcao.Direita ? -1 : 1);
            //animador.SetTrigger("levouDano");
        }

        vidaAtual += valor;

        if (vidaAtual > vidaMaxima)
        {
            vidaAtual = vidaMaxima;
        }

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }
    
    private void Morrer()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (colisao.tag == "Inimigo")
        {
            if ((transform.position.y - controlador.colisor.size.y/2) > colisao.transform.position.y)
            {
                colisao.gameObject.SendMessage("MudarVida", -1);
                velocidade.y = velocidadePulo;
                controlador.Mover(velocidade * Time.deltaTime);
            }
            else
                MudarVida(-1);
        }

        if (colisao.tag == "Moeda")
        {
            moedas++;
            Destroy(colisao.gameObject);
        }

        if (colisao.tag == "Limite")
        {
            Morrer();
        }

        if (colisao.tag == "FimFase")
        {
            colisao.gameObject.SendMessage("MudarNivel");
        }
    }

    private void AtualizaOlhar(float inputHorizontal)
    {
        // Checa se o Input é pra direita
        if (inputHorizontal == 1)
        {
            // Checa se o olhar NÃO é pra direita, e então muda
            if (direcaoOlhar != Direcao.Direita)
            {
                direcaoOlhar = Direcao.Direita;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
        else if (inputHorizontal == -1)
        {
            // Checa se o olhar NÃO é pra esquerda, e então muda
            if (direcaoOlhar != Direcao.Esquerda)
            {
                direcaoOlhar = Direcao.Esquerda;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
    }
}

public enum Direcao {Direita, Esquerda};
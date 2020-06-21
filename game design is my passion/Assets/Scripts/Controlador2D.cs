using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Faz com que a classe apenas possa ser posta em um objeto caso o componente BoxCollider2D exista dentro dela. Caso o componente não exista, ao adicionar a nossa classe ele será adicionado também.
[RequireComponent(typeof(BoxCollider2D))]
public class Controlador2D : MonoBehaviour
{
    // Máscara que usaremos para detectar nossas plataformas. Apenas os objetos com essa máscara serão atingidos pelos nossos raios.
    public LayerMask mascaraPlataforma;
    // "Pele" para diminuirmos o ponto de origem dos nossos raios. Precisamos para evitar colisões estranhas e bugs relacionados a isso.
    // Esse valor é declarado como uma constante, e portanto, não podemos modificar ela durante o código. Imagine como se fosse PI em uma conta matemática.
    const float larguraPele = 0.015f;

    // Quantidade de raios que iremos soltar horizontal e verticalmente.
    public int quantidadeRaiosHorizontais;
    public int quantidadeRaiosVerticais;

    // O espaço entre os raios horizontais e verticais que iremos lançar. É calculado quando o objeto é iniciado.
    float espacamentoRaiosHorizontais;
    float espacamentoRaiosVerticais;

    // Referência para nosso componente BoxCollider2D.
    [HideInInspector]
    public BoxCollider2D colisor;

    // Objeto que contém nossa estrutura para definir as origens dos nossos raios. É calculado toda vez que decidimos nos movimentar.
    private OrigensRaycast origensRaycast;

    // Objeto que contém nossa estrutura para armazenar informações sobre colisões.
    public InformacoesColisao colisoes;

    // O método Awake() é chamado assim que o objeto entra em cena, diferentemente de Start() que só é chamado quando o script está habilitado.
    // Você pode procurar mais informações sobre a diferença entre Start, Awake, Update, FixedUpdate e LateUpdate na documentação de Unity.
    // https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html
    void Awake()
    {
        // Inicializa nosso colisor ao pegar um componente (BoxCollider2D) do nosso objeto.
        colisor = GetComponent<BoxCollider2D>();

        // Calcula o espaçamento entre os raios assim que o objeto é criado.
        CalcularEspacamentoRaios();
    }

    // Método para movimentar nosso personagem.
    public void Mover(Vector3 distancia)
    {
        // Antes de nos movimentar precisamos atualizar as origens dos nossos raios.
        AtualizarOrigensRaycast();

        // Resetamos as colisões (já que podemos ter novas colisões agora)
        colisoes.Resetar();

        // Apenas checa por colisões horizontais se a nossa distancia.x for diferente de 0 (afinal, se for 0 não estamos nos movendo)
        if (distancia.x != 0)
        {
            ColisoesHorizontais(ref distancia);
        }

        // Apenas checa por colisões verticais se a nossa distancia.y for diferente de 0
        if (distancia.y != 0)
        {
            ColisoesVerticais(ref distancia);
        }

        // Muda a posição do personagem
        // https://docs.unity3d.com/ScriptReference/Transform.Translate.html
        transform.Translate(distancia);
    }

    // Checaremos por colisões verticais neste método, atualizando a nossa velocidade caso nossos raios atinjam algum objeto.
    private void ColisoesVerticais(ref Vector3 distancia)
    {
        // Definimos a direção que estamos indo (-1 sendo para baixo e 1 para cima).
        // Utilizamos Mathf.Sign para obter o sinal da nossa distancia.y e saber se estamos caíndo ou subindo.
        float direcaoY = Mathf.Sign(distancia.y);

        // Para o tamanho, utilizaremos Mathf.Abs para retornar o valor absoluto de distancia.y (já que não podemos ter um tamanho negativo).
        // Somamos com a largura de nossa pele, para que os raios saiam de fora do nosso colisor.
        float distanciaRaio = Mathf.Abs(distancia.y) + larguraPele;

        // https://www.codigofonte.com.br/artigos/estrutura-de-repeticao-for-para-
        // https://docs.microsoft.com/pt-br/dotnet/csharp/language-reference/keywords/for
        // https://www.tutorialsteacher.com/csharp/csharp-for-loop
        // Nesse laço de repetição, iremos executar um número de vezes igual à nossa quantidade de raios verticais.
        for (int i = 0; i < quantidadeRaiosVerticais; i++)
        {
            // https://docs.microsoft.com/pt-br/dotnet/csharp/language-reference/operators/conditional-operator
            // https://www.tutorialsteacher.com/csharp/csharp-ternary-operator
            // http://www.macoratti.net/14/11/c_tern1.htm
            // Definimos a origem do nosso raio dependendo de nossa direção.
            Vector2 origemRaio = (direcaoY == -1) ? origensRaycast.inferiorEsquerdo : origensRaycast.superiorEsquerdo;
            
            // Mudamos a posição da nossa origem dependendo de qual iteração estamos. Experimente mudar o Vector2.right para left, down, up, etc... e veja o resultado.
            origemRaio += Vector2.right * (espacamentoRaiosVerticais * i + distancia.x);

            // Invocamos nosso raio.
            RaycastHit2D hit = Physics2D.Raycast(origemRaio, Vector2.up * direcaoY, distanciaRaio, mascaraPlataforma);

             // Desenhamos o raio na tela, experimente mudar Vector2.up para left, down, e right e veja o resultado.
            Debug.DrawRay(origemRaio, Vector2.up * direcaoY * distanciaRaio, Color.red);

            // Caso nosso hit seja verdadeiro (ou seja, acertou alguma coisa), executamos o código abaixo
            if (hit)
            {
                // Definimos a nossa distancia.y como a distância da origem até o colisor (hit.distance), 
                // subtraímos a nossa pele e multiplicamos a direcaoY, para que não percamos a direção da nossa movimentação.
                distancia.y = (hit.distance - larguraPele) * direcaoY;

                // Definimos que o raio é igual à distância do hit, para evitar que nosso personagem se movimente para dentro do nosso objeto colidido.
                distanciaRaio = hit.distance;

                // Pense no que o código abaixo faz.
                colisoes.abaixo = direcaoY == -1;
                colisoes.acima = direcaoY == 1;
            }
        }
    }

    // Após observar o código para colisões verticais, tente descobrir o que o código abaixo faz.
    private void ColisoesHorizontais(ref Vector3 distancia)
    {
        float direcaoX = Mathf.Sign(distancia.x);
        float distanciaRaio = Mathf.Abs(distancia.x) + larguraPele;

        for (int i = 0; i < quantidadeRaiosHorizontais; i++)
        {
            Vector2 origemRaio = (direcaoX == -1) ? origensRaycast.inferiorEsquerdo : origensRaycast.inferiorDireito; 
            origemRaio += Vector2.up * (espacamentoRaiosHorizontais * i);

            RaycastHit2D hit = Physics2D.Raycast(origemRaio, Vector2.right * direcaoX, distanciaRaio, mascaraPlataforma);

            Debug.DrawRay(origemRaio, Vector2.right * direcaoX * distanciaRaio, Color.red);

            if (hit)
            {
                distancia.x = (hit.distance - larguraPele) * direcaoX;
                distanciaRaio = hit.distance;

                colisoes.esquerda = direcaoX == -1;
                colisoes.direita = direcaoX == 1;
            }
        
        }
    }

    private void AtualizarOrigensRaycast()
    {
        Bounds limites = colisor.bounds;
        limites.Expand(larguraPele * -2);

        origensRaycast.inferiorEsquerdo = new Vector2(limites.min.x, limites.min.y);
        origensRaycast.inferiorDireito = new Vector2(limites.max.x, limites.min.y);
        origensRaycast.superiorEsquerdo = new Vector2(limites.min.x, limites.max.y);
        origensRaycast.superiorDireito = new Vector2(limites.max.x, limites.max.y);
    }

    private void CalcularEspacamentoRaios()
    {
        Bounds limites = colisor.bounds;
        limites.Expand (larguraPele * -2);

        quantidadeRaiosHorizontais = Mathf.Clamp(quantidadeRaiosHorizontais, 2, int.MaxValue);
        quantidadeRaiosVerticais = Mathf.Clamp(quantidadeRaiosVerticais, 2, int.MaxValue);

        espacamentoRaiosHorizontais = limites.size.y / (quantidadeRaiosHorizontais - 1);
        espacamentoRaiosVerticais = limites.size.x / (quantidadeRaiosVerticais - 1);
    }

    // Pensem em estruturas como classes, mas mais simples.
    private struct OrigensRaycast
    {
        public Vector2 superiorEsquerdo, superiorDireito;
        public Vector2 inferiorEsquerdo, inferiorDireito;
    }

    public struct InformacoesColisao {
        public bool acima, abaixo;
        public bool esquerda, direita;

        public void Resetar()
        {
            acima = abaixo = false;
            esquerda = direita = false;
        }
    }
}
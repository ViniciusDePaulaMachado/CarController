using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{

    private Rigidbody carro_rigidbody;
    [SerializeField] private WheelCollider[] rodas;
    [SerializeField] private Transform[] rodas_mesh;
    [SerializeField] private AudioClip ronco_motor;
    [SerializeField] private AudioSource config_som;
    
    [Space]
    [Header("Especificacoes direcao")]
    [Space]
    [SerializeField] [Range(5, 50f)] private byte angulo_direcao;
    [Space]
    [SerializeField] [Range(0, 100f)]  private byte ajuda_derecao = 15;
    [Space]
    [SerializeField] [Range(0.01f,1f)] private float angulo_akerman;
    [Space]
    [SerializeField] [Range(0, 50f)] private byte sensibiliade_direcao;
    [Space]

    [Header("Especificacoes")]
    [Space]
    [SerializeField] private int peso_veiculo;
    [Space]
    [SerializeField] private Vector3 centro_massa;   
    [Space]

    [Header("Especificacores do Motor")]
    [Space]
    [SerializeField] private AnimationCurve curva_torque;
    [Space]
    [SerializeField] private ushort max_torque;
    [SerializeField] private ushort min_torque;
    [Space]
    [SerializeField] private ushort max_torque_rpm;
    [SerializeField] private ushort min_torque_rpm;
    [Space]
    [SerializeField] private ushort min_Rpm;
    [SerializeField] private ushort max_Rpm;
    [Space]
    [SerializeField] private ushort forca_freio;

    [Header("Especificacores da transmissao")]
    [Space]
    [SerializeField] private bool transmissa_automatica = false;
    [Space]
    [SerializeField] private byte quantidadde_marchas;
    [Space]
    [SerializeField] private float[] relacao_marchas;
    [Space]
    [SerializeField] private float relacao_diferencial;
    [Space]

    //**********[Variaveis auxiliares do carro]**********//
    
    private float rpmMotor;
    float velocidadeDoCarro;

    //**********[Variaveis auxiliares da direcao]**********//
    private float grau_difecao;
    private float input_Vertical;
    private float input_Horizontal;

    //**********[Variaveis auxiliares da transmissao]**********//

    private byte marcha_atual; // inplementar

    //**********[Variaveis auxiliares do som]**********//

    private float pith;

    private void Awake()
    {
        AplicarEConfigurarSom();
    }
    void Start()
    {
        AplicarCurvaPotencia();
        ConfigurarRIgidBody();
    }

    void Update()
    {
        Direcao();
        ControleDeInputs();
        Gerenciadoretransmissao(transmissa_automatica);
    }

    void FixedUpdate()
    {
        Movimentar();
        AtualizarAnimRodas();
        freiar();
        SomDoMotor();
    }

    void ControleDeInputs()
    {
        input_Vertical = Input.GetAxis("Vertical");
        input_Vertical = Mathf.Clamp(input_Vertical, -1f, 1f);

        input_Horizontal = Input.GetAxis("Horizontal");
        input_Horizontal = Mathf.Clamp(input_Horizontal, -1f, 1f);
    }

    void Direcao()
    {
        float angulo = angulo_direcao / velocidadeDoCarro * Mathf.Abs(-100 + (ajuda_derecao-1));

        if(angulo > angulo_direcao)
        {
            angulo = angulo_direcao;
        }

        grau_difecao = Mathf.Lerp(grau_difecao, angulo * input_Horizontal, Time.deltaTime * sensibiliade_direcao);

        if (input_Horizontal > -0.01) //direita
        {
            rodas[0].steerAngle = grau_difecao; // esquerda
            rodas[1].steerAngle = grau_difecao - ((grau_difecao * angulo_akerman - (grau_difecao)) / 2); // direita
            Debug.Log("Esquerda: " + rodas[0].steerAngle + "Direita: " + rodas[1].steerAngle);
        }
        else if(input_Horizontal < 0.01)  // esquerda
        {
            rodas[0].steerAngle = grau_difecao - ((grau_difecao * angulo_akerman - (grau_difecao)) / 2); // esquerda
            rodas[1].steerAngle = grau_difecao; // direita
            Debug.Log("Esquerda: " + rodas[0].steerAngle + "Direita: " + rodas[1].steerAngle);
        }
    }

    void Movimentar()
    {
        velocidadeDoCarro = carro_rigidbody.velocity.magnitude * 3.6f;
        
        //teste
        rpmMotor = relacao_diferencial * relacao_marchas[marcha_atual] * ((rodas[2].rpm + rodas[3].rpm) / 2);

        if (rpmMotor < min_Rpm) 
        { 
            rpmMotor = min_Rpm; 

        }

        if(marcha_atual == 0 && velocidadeDoCarro < 20 && input_Vertical > 0.1)
        {
            rpmMotor = min_torque_rpm;
        }
        
        if (input_Vertical > 0 && rpmMotor < max_Rpm - 100)
        {
            //Debug.Log(input_Vertical);
            float torque = (curva_torque.Evaluate(rpmMotor) * (relacao_marchas[marcha_atual])) * this.input_Vertical;
             
            float potenciaReal = (((curva_torque.Evaluate(rpmMotor) * rpmMotor ) * 2) * Mathf.PI) / 60;

            //Debug.Log("Torque: " + (torque * 0.9f).ToString("0000") + " Potencia: " + potenciaReal / 735.499f);

            rodas[2].motorTorque = torque * 0.9f;
            rodas[3].motorTorque = torque * 0.9f;
        }
        else
        {
            rodas[2].motorTorque = 0;
            rodas[3].motorTorque = 0;
        }
    }

    void freiar()
    {
        if (input_Vertical < 0.01f)
        {
            float intencidadeDoFreio = -forca_freio * input_Vertical;
            rodas[0].brakeTorque = intencidadeDoFreio;
            rodas[1].brakeTorque = intencidadeDoFreio;
            rodas[2].brakeTorque = intencidadeDoFreio/2;
            rodas[3].brakeTorque = intencidadeDoFreio/2;
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            rodas[2].brakeTorque = forca_freio * 2;
            rodas[3].brakeTorque = forca_freio * 2;
            Debug.Log("Freio de mao");
            return;
        }

        rodas[0].brakeTorque = 0;
        rodas[1].brakeTorque = 0;
        rodas[2].brakeTorque = 0;
        rodas[3].brakeTorque = 0;
    }

    void SubirMarcha()
    {
        if (marcha_atual < quantidadde_marchas-1)
        {
            marcha_atual++;
        }
    }

    void DescerMarcha()
    {
        if (marcha_atual > 0)
        {
            marcha_atual--;
        }
    }

    void Gerenciadoretransmissao(bool transmissaoAutomatica)
    {
        if(rpmMotor >= max_Rpm && transmissaoAutomatica || Input.GetKeyDown(KeyCode.UpArrow))
        {
            SubirMarcha();
        }

        float rpmVoltarMarcha = min_Rpm + min_Rpm * 0.2f;

        if(rpmMotor < rpmVoltarMarcha && transmissaoAutomatica || Input.GetKeyDown(KeyCode.DownArrow))
        {
            DescerMarcha();
        }
    }

    void AtualizarAnimRodas()
    {
        for(byte i = 0; i < rodas.Length; i++)
        {
            rodas[i].GetWorldPose(out Vector3 pos, out Quaternion quat);

            rodas_mesh[i].SetPositionAndRotation(pos,quat);
        }
    }

    void SomDoMotor()
    {
        pith = Mathf.Lerp(pith, rpmMotor / 5800, Time.deltaTime * 5);

        config_som.pitch = pith;
        if(Input.GetAxis("Vertical") <= 0)
        {
            config_som.volume = 0.8f;
        }
        else
        {
            config_som.volume = 0.8f; ;
        }
    }

    //*****************[Start]*****************//
    void AplicarCurvaPotencia()
    {
        curva_torque = new AnimationCurve(new Keyframe(0, 0), new Keyframe(min_torque_rpm, min_torque),
                                                            new Keyframe(max_torque_rpm, max_torque), new Keyframe(max_Rpm, max_torque / 2));
    }

    void ConfigurarRIgidBody()
    {
        carro_rigidbody = gameObject.GetComponent<Rigidbody>();
        carro_rigidbody.mass = peso_veiculo;
        carro_rigidbody.centerOfMass = centro_massa;
        //Debug.Log("Centro de massa: " + carro_rigidbody.centerOfMass);
    }

    void AplicarEConfigurarSom()
    {
        config_som = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
        config_som.clip = ronco_motor;
        config_som.Play(1);
        config_som.loop = true;
    }

    public float getRPM()
    {
        return rpmMotor;
    }

    public float getVelocidadeKM()
    {
        return velocidadeDoCarro;
    }

    public byte getMarchaAtual()
    {
        return marcha_atual;
    }
    // set inputs
    public void setinputVerticalMoble(float inputV)
    {
        this.input_Vertical = inputV;
    }

    public void setinputHorizontalMoble(float inputH)
    {
        this.input_Horizontal += inputH;
    }

    // tunnar carro direcao

    public void setAnguloDirecao(byte angulo)
    {
        this.angulo_direcao = angulo;
    }

    public byte getAnguloDirecao()
    {
        return this.angulo_direcao;
    }

    public void setAnguloAkerman(float porcentagem)
    {
        this.angulo_akerman = Mathf.Clamp(porcentagem, 0, 1f);
    }

    public float getAnguloDeKaterman()
    {
        return this.angulo_akerman;
    }

    public void setAjudaNaDirecao(byte ajudaDirecao)
    {
        this.ajuda_derecao = ajudaDirecao;
    }

    public byte getAjudaNaDirecao()
    {
        return this.ajuda_derecao;
    }
}
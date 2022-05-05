using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{

    [SerializeField] WheelCollider[] rodas;
    [SerializeField] Transform[] rodas_mesh;
    Rigidbody carro_rigidbody;
    [Space]
    [Header("Direcao")]
    [Space]
    [SerializeField] byte angulo_direcao;
    [SerializeField] byte sensibiliade_direcao;
    [Space]
    [Header("Especificacoes")]
    [Space]
    [SerializeField] int peso_veiculo;
    [SerializeField] Transform centro_massa;    
    [Space]
    [SerializeField] AnimationCurve potencia;
    [SerializeField] int max_torque;
    [SerializeField] int min_torque;
    [Space]
    [SerializeField] int max_torque_rpm;
    [SerializeField] int min_torque_rpm;
    [Space]
    [SerializeField] int min_Rpm;
    [SerializeField] int max_Rpm;
    [Space]
    [SerializeField] int forca_freio;

    public bool _debug;

    //**********[Variaveis auxiliares do carro]**********//
    
    private float rpm;
    float velocidade;

    //**********[Variaveis auxiliares da direcao]**********//
    private float _sensibiliade_direcao;


    void Start()
    {
        AplicarCurvaPotencia();
        ConfigurarRIgidBody();

    }

    void Update()
    {
        Direcao();
    }

    private void FixedUpdate()
    {
        Movimentar();
        AtualizarAnimRodas();
        freiar();
        Debug(_debug);
    }

    void Direcao()
    {
        _sensibiliade_direcao = Mathf.Lerp(_sensibiliade_direcao, angulo_direcao * Input.GetAxis("Horizontal"), Time.deltaTime * sensibiliade_direcao);
        rodas[0].steerAngle = rodas[1].steerAngle = _sensibiliade_direcao;
    }

    void Movimentar()
    {
        velocidade = carro_rigidbody.velocity.magnitude * 3.6f;

        rpm = velocidade * 100f;

        if (rpm < min_Rpm) 
        { 
            rpm = min_Rpm; 
        }
        if( rpm > max_Rpm)
        {
            rodas[2].motorTorque = rodas[3].motorTorque = 0;
        }
        else if(Input.GetAxis("Vertical") > 0)
        {
            rodas[2].motorTorque = rodas[3].motorTorque = potencia.Evaluate(rpm) * Input.GetAxis("Vertical");
        }
    }

    void freiar()
    {
        if (Input.GetAxis("Vertical") < 0)
        {
            rodas[0].brakeTorque = rodas[1].brakeTorque = forca_freio * Input.GetAxis("Vertical");
        }
        else
        {
            rodas[0].brakeTorque = rodas[1].brakeTorque = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rodas[2].brakeTorque = rodas[3].brakeTorque = forca_freio * 10;
        }
    }

    void AtualizarAnimRodas()
    {
        for(byte i = 0; i < rodas.Length -1; i++)
        {
            rodas[i].GetWorldPose(out Vector3 pos, out Quaternion quat);

            rodas_mesh[i].SetPositionAndRotation(pos,quat);
        }
    }

    void Debug(bool debug)
    {
        if(debug)
        print("Rpm: " + rpm + " KM/H: " + (int)velocidade);
    }

    //*****************[Strat]*****************//

    void AplicarCurvaPotencia()
    {
        potencia = new AnimationCurve(new Keyframe(0, 0), new Keyframe(min_torque_rpm, min_torque),
                                                            new Keyframe(max_torque_rpm, max_torque), new Keyframe(max_Rpm, max_torque / 2));
    }

    void ConfigurarRIgidBody()
    {
        carro_rigidbody = gameObject.GetComponent<Rigidbody>();
        carro_rigidbody.mass = peso_veiculo;
        carro_rigidbody.centerOfMass = new Vector3(0, -0.5f, 0);
    }
}

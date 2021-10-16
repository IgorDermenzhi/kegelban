using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Text, Image, Button

public class Controls : MonoBehaviour
{
    private const float MIN_FORCE = 1000f;
    private const float MAX_FORCE = 2000f;
    private GameObject Indicator;
    
    private GameObject Ball;
    private Rigidbody ballRigidbody;
    private Vector3 ballStartPosition;
    private bool IsBallMoving;

    private GameObject Arrow;
    private GameObject ArrowTail;
    private float arrowAngle; // угол поворота стрелки
    private float maxArrowAngle = 20f; // граничный угол поворота

    private Image ForceIndicator;

    private Text GameStat;
    private int attempt;

    private GameObject GameMenu;
     

    // Start is called before the first frame update
    void Start()
    {
        GameMenu = GameObject.Find("Menu");
        Menu.MenuMode = MenuMode.Start;

        attempt = 0;
        GameStat = GameObject.Find("GameStat").GetComponent<Text>();
        // Получаем ссылку на компонент Image объекта ForceIndicator
        ForceIndicator = GameObject.Find("ForceIndicator").GetComponent<Image>();
        Arrow = GameObject.Find("Arrow");
        ArrowTail = GameObject.Find("ArrowTail");
        arrowAngle = 0f;

        Indicator = GameObject.Find("Indicator");

        // Находим шарик
        Ball = GameObject.Find("Ball");
        ballStartPosition = Ball.transform.position;
        // сохраняем ссылку на его твердое тело
        // Толкнуть шарик - приложить силу к его твердому телу
        ballRigidbody = Ball.GetComponent<Rigidbody>();
        IsBallMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Menu.IsActive) return;
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameMenu.SetActive(true);
            Menu.IsActive = true;
            Menu.MenuMode= MenuMode.Pause;
        }
            #region Остановка шарика
            if (ballRigidbody.velocity.magnitude < 0.2f && IsBallMoving)
        {
            // считаем за остановку
            Debug.Log("Ball stopped");
            IsBallMoving = false;
            Ball.transform.position = ballStartPosition;
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;

            int kegelsUp = 0;
            int kegelsDown = 0;
            foreach(GameObject kegel in
                GameObject.FindGameObjectsWithTag("Kegel"))
            {
                // стоит y = 0
                // лежти y > 0
                if(kegel.transform.position.y > 0.1 
                    || Mathf.Abs(kegel.transform.rotation.x) > 0.01
                    || Mathf.Abs(kegel.transform.rotation.z) > 0.01)
                {
                    kegel.SetActive(false);
                    kegelsDown++;
                }
                else
                {
                    kegelsUp++;
                    // Если кегля недоупала, то выровняем ее
                    kegel.transform.rotation = Quaternion.Euler(0, 0, 0);
                    kegel.transform.position.Set(kegel.transform.position.x, 0, kegel.transform.position.z);
                }
                // Debug.Log(kegel.transform.position);
            }
            Arrow.SetActive(true);
            Indicator.SetActive(true);
            // Выводим статистику
            attempt++;
            GameStat.text += "\n" + attempt + "  " + Clock.StringValue + "  " + kegelsDown + "  " + kegelsUp;
        }
        #endregion

        #region Запуск шарика
        if (Input.GetKeyDown(KeyCode.Space) && !IsBallMoving)
        {
            
            Vector3 forceDirection = Arrow.transform.forward;
            // Величина силы - по индикатору (MIN-MAX)
            float forceFactor = MIN_FORCE + (MAX_FORCE - MIN_FORCE) * ForceIndicator.fillAmount;
            ballRigidbody.AddForce(forceFactor * forceDirection);
            ballRigidbody.velocity = forceDirection * 0.2f;
            IsBallMoving = true;
            Arrow.SetActive(false);
            Indicator.SetActive(false);

        }
        #endregion

        #region Вращение стрелки
        if (Input.GetKey(KeyCode.LeftArrow) && arrowAngle > -maxArrowAngle)
        {
            Arrow.transform.RotateAround(       // Вращение
                ArrowTail.transform.position,   // Центр вращения
                Vector3.up,                     // Ось вращения
                -0.01f                              // Угол вращения
            );
            arrowAngle -= 0.01f;
        }
        if (Input.GetKey(KeyCode.RightArrow) && arrowAngle < maxArrowAngle)
        {
            Arrow.transform.RotateAround(       
                ArrowTail.transform.position,   
                Vector3.up,                     
                0.01f                              
            );
            arrowAngle += 0.01f;
        }
        #endregion


        #region Индикатор силы
        if (Input.GetKey(KeyCode.UpArrow) && ForceIndicator.fillAmount < 1f)
        {
            // ForceIndicator.fillAmount += 0.01f;
            float val = ForceIndicator.fillAmount + Time.deltaTime / 2;
            if(val <= 1)
            ForceIndicator.fillAmount = val;
        }
        if (Input.GetKey(KeyCode.DownArrow) && ForceIndicator.fillAmount >  0.1f)
        {
            // ForceIndicator.fillAmount -= 0.01f;
            float val = ForceIndicator.fillAmount - Time.deltaTime / 2;
            if (val >= .1f)
                ForceIndicator.fillAmount = val;
        }
        #endregion

    }

    // обработчик кнопки меню
    public void PlayClick()
    {
        //Debug.Log("Click");
        GameMenu.SetActive(false);
        Menu.IsActive = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI; // Text, Image, Button

public class Controls : MonoBehaviour
{
    private const float MIN_FORCE = 1000f;
    private const float MAX_FORCE = 2000f;
    private const string BEST_RES_FILE = "best.xml";

    private GameObject Indicator;
    
    private GameObject Ball;
    private Rigidbody ballRigidbody;
    private Vector3 ballStartPosition;
    private bool IsBallMoving;

    private GameObject Arrow;
    private GameObject ArrowTail;
    private float arrowAngle; // ���� �������� �������
    private float maxArrowAngle = 20f; // ��������� ���� ��������

    private Image ForceIndicator;

    private Text GameStat;
    private int attempt;

    private GameObject GameMenu;

    private List<GameResult> bestResults; // ������� ��������

    private GameObject DisplayResults;
    private Text TextBestResults;

    // Start is called before the first frame update
    void Start()
    {
        GameMenu = GameObject.Find("Menu");
        Menu.MenuMode = MenuMode.Start;
        DisplayResults = GameObject.Find("DisplayResults");
        TextBestResults = GameObject.Find("Results").GetComponent<Text>();

        attempt = 0;
        GameStat = GameObject.Find("GameStat").GetComponent<Text>();
        // �������� ������ �� ��������� Image ������� ForceIndicator
        ForceIndicator = GameObject.Find("ForceIndicator").GetComponent<Image>();
        Arrow = GameObject.Find("Arrow");
        ArrowTail = GameObject.Find("ArrowTail");
        arrowAngle = 0f;

        Indicator = GameObject.Find("Indicator");

        // ������� �����
        Ball = GameObject.Find("Ball");
        ballStartPosition = Ball.transform.position;
        // ��������� ������ �� ��� ������� ����
        // �������� ����� - ��������� ���� � ��� �������� ����
        ballRigidbody = Ball.GetComponent<Rigidbody>();
        IsBallMoving = false;
        LoadBestResults();
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
        if (Input.GetKeyDown(KeyCode.B))
        {
            GameMenu.SetActive(true);
            Menu.IsActive = true;
            Menu.MenuMode = MenuMode.Pause;
            DisplayResults.SetActive(true);
        }

        #region ��������� ������
        if (ballRigidbody.velocity.magnitude < 0.2f && IsBallMoving)
        {
            // ������� �� ���������
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
                // ����� y = 0
                // ����� y > 0
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
                    // ���� ����� ���������, �� ��������� ��
                    kegel.transform.rotation = Quaternion.Euler(0, 0, 0);
                    kegel.transform.position.Set(kegel.transform.position.x, 0, kegel.transform.position.z);
                }
                // Debug.Log(kegel.transform.position);
            }
            Arrow.SetActive(true);
            Indicator.SetActive(true);
            // ������� ����������
            attempt++;
            GameStat.text += "\n" + attempt + "  " + Clock.StringValue + "  " + kegelsDown + "  " + kegelsUp;
        }
        #endregion

        #region ������ ������
        if (Input.GetKeyDown(KeyCode.Space) && !IsBallMoving)
        {
            
            Vector3 forceDirection = Arrow.transform.forward;
            // �������� ���� - �� ���������� (MIN-MAX)
            float forceFactor = MIN_FORCE + (MAX_FORCE - MIN_FORCE) * ForceIndicator.fillAmount;
            ballRigidbody.AddForce(forceFactor * forceDirection);
            ballRigidbody.velocity = forceDirection * 0.2f;
            IsBallMoving = true;
            Arrow.SetActive(false);
            Indicator.SetActive(false);

        }
        #endregion

        #region �������� �������
        if (Input.GetKey(KeyCode.LeftArrow) && arrowAngle > -maxArrowAngle)
        {
            Arrow.transform.RotateAround(       // ��������
                ArrowTail.transform.position,   // ����� ��������
                Vector3.up,                     // ��� ��������
                -0.01f                              // ���� ��������
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


        #region ��������� ����
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

    // ���������� ������ ����
    public void PlayClick()
    {
        //Debug.Log("Click");
        GameMenu.SetActive(false);
        Menu.IsActive = false;
        DisplayResults.SetActive(false);
    }

    private void LoadBestResults()
    {
        //���� � ������������
        if(File.Exists(BEST_RES_FILE))
        {
            using(StreamReader reader = new StreamReader(BEST_RES_FILE))
            {
                XmlSerializer serializer = new XmlSerializer(
                    typeof(List<GameResult>));
                bestResults = (List<GameResult>)serializer.Deserialize(reader);
                bestResults.Sort();
            }
            foreach(var res in bestResults)
            {
                // Debug.Log(res);
                TextBestResults.text += "\n" + res;
            }
        }
        else
        {
            bestResults = new List<GameResult>();
            bestResults.Add(new GameResult { Balls = 20, Time = 200 });
            bestResults.Add(new GameResult { Balls = 30, Time = 300 });
            bestResults.Add(new GameResult { Balls = 10, Time = 100 });
        }
            using(StreamWriter writer = new StreamWriter(BEST_RES_FILE))
            {
                XmlSerializer serializer = new XmlSerializer(
                    bestResults.GetType());
                serializer.Serialize(writer, bestResults);
            }
    }
}


public class GameResult : System.IComparable<GameResult>
{
        public int Balls { get; set; }  // ������
        public float Time { get; set; } // ����� ������

    public int CompareTo(GameResult y)
    {
        if (this.Balls < y.Balls) return -1;
        else if (this.Balls == y.Balls)
        {
            if (this.Time < y.Time) return -1;
            else if (this.Time == y.Time) return 0;
        }
        return 1;
    }

    public override string ToString()
    {
        return "Balls: " + Balls + ", Time: " + Time;
    }

}



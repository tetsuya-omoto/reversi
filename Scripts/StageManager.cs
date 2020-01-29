using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class StageManager : MonoBehaviour
{
    public enum eStoneState//石の状態
    {
        EMPTY,//石が空
        WHITE,//石の上が白
        BLACK//石の上が黒
    };
    private eStoneState turn = eStoneState.BLACK;//ターン。最初は黒
    public GameObject firstStone;//置いた石
    private GameObject[,] firstStoneState = new GameObject[squareZ, squareX];//置いた石の座標
    private StoneManager[,] stoneManagers = new StoneManager[squareZ, squareX];//石のシリンダーとマテリアルの状態
    private eStoneState[,] stoneState = new eStoneState[squareZ, squareX];//石が空か白か黒か
    private int x;
    private int z;//タップした座標
    private bool beRay = false;
    public Camera mainCamera;//カメラ取得用変数
    const int squareX = 8;//盤上のx(横)座標
    const int squareZ = 8;//盤上のz(縦)座標
    class TurnableStone {
        public int turnZ;
        public int turnX;
        public TurnableStone(int z, int x)
        {
            turnZ = z;
            turnX = x;
        }
    }//ひっくり返すことができる駒の位置

    int[] TURN_CHECK_X = new int[] { -1, -1, 0, 1, 1, 1, 0, -1 };
    int[] TURN_CHECK_Z = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };//石の隣8方向
    public int whiteScore;//白の枚数
    public int blackScore;//黒の枚数
    public GameObject whiteWinText;//白が勝ったときに出るテキスト
    public GameObject blackWinText;//黒が勝ったときに出るテキスト
    public Text whiteScoreText;//白の枚数を表示するテキスト
    public Text blackScoreText;//黒の枚数を表示するテキスト
    public Text timerText;//タイマーを表示
	private float seconds;//制限時間
    public AudioClip putStoneSE;//石を打つ音
    private AudioSource audioSource;
    public bool isAI = false;//AIのターンかどうか




    void Start()
    {
        seconds = 7;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        audioSource = gameObject.GetComponent<AudioSource>();
        for (int i = 0; i < squareZ; i++)
        {
            for (int j = 0; j < squareX; j++)
            {
                // 石を64枚EMPTYで生成
                GameObject stone = GameObject.Instantiate<GameObject>(firstStone);
                StoneManager stoneManager = stone.GetComponent<StoneManager>();

                stone.transform.position = new Vector3(j, 1, i);
                firstStoneState[i, j] = stone;
                stoneManagers[i, j] = stoneManager;
                stoneState[i, j] = eStoneState.EMPTY;
            }
                stoneState[3, 3] = eStoneState.WHITE;
                stoneState[3, 4] = eStoneState.BLACK;
                stoneState[4, 3] = eStoneState.BLACK;
                stoneState[4, 4] = eStoneState.WHITE;
        }
        // AISquarePoint();
        whiteScore = 2;
        blackScore = 2;

    }

    void Update()
    {
        seconds -= Time.deltaTime;
        timerText.text = ((int)(seconds)).ToString("0");
        if (seconds <= 0f) {
            turn = ((turn == eStoneState.BLACK) ? eStoneState.WHITE : eStoneState.BLACK);
            isAI = ((isAI == false) ? true : false);
            seconds = 7;
            Debug.Log(isAI);
        }
        if (!isAI) {
            PutStone();
        }
        for (int i = 0; i < squareZ; i++)
        {
            for (int j = 0; j < squareX; j++)
            {
                // 石の状態を確認
                stoneManagers[i, j].SetState(stoneState[i, j]);
            }
        }
        if (isAI){
            AIPutStone();
        }

        CheckWinner();
        blackScoreText.text = "黒" + blackScore + "枚";
        whiteScoreText.text = "白" + whiteScore + "枚";

    }
    //タップで石を置く処理
    public void PutStone(){
        if (Input.GetMouseButtonDown(0))
        {
            //マウスのポジションを取得してRayに代入
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            //マウスのポジションからRayを投げて何かに当たったらhitに入れる
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))
            {
                //x,zの値を取得
                x = (int)hit.collider.gameObject.transform.position.x;
                z = (int)hit.collider.gameObject.transform.position.z;

                if (0 <= x && x < squareX && 0 <= z && z < squareZ &&
                    stoneState[z, x] == eStoneState.EMPTY && Turn(false) > 0)
                {
                    audioSource.PlayOneShot(putStoneSE);

                    stoneState[z, x] = turn;
                    if (turn == eStoneState.BLACK) {
                        blackScore ++;
                    }
                    else if (turn == eStoneState.WHITE) {
                        whiteScore ++;
                    }
                    Turn(true);
                    
                    turn = ((turn == eStoneState.BLACK) ? eStoneState.WHITE : eStoneState.BLACK);
                    isAI = true;
                }
            }
        }
    }
    //ひっくり返す石を取得する処理
        int Turn(bool isTurn)
    {
        // 相手の石の色
        eStoneState enemyColor = ((turn == eStoneState.BLACK) ? eStoneState.WHITE : eStoneState.BLACK);

        bool isTurnable = false;// ひっくり返すことができるかどうか
        List<TurnableStone> turnableStoneList = new List<TurnableStone>();//ひっくり返す石のリスト
        int count = 0;
        int turnCount = 0;

        int plusX = 0, plusZ = 0;
        for (int i = 0; i < TURN_CHECK_X.Length; i++)
        {
            int _x = x;
            int _z = z;

            plusX = TURN_CHECK_X[i];
            plusZ = TURN_CHECK_Z[i];
            isTurnable = false;
            turnableStoneList.Clear();
            while (true)
            {
                _x += plusX;
                _z += plusZ;
                if (!(0 <= _x && _x < squareX && 0 <= _z && _z < squareZ))
                {
                    break;
                }
                if (stoneState[_z, _x] == enemyColor)
                {
                    // ひっくり返す対象
                    turnableStoneList.Add(new TurnableStone(_z, _x));
                }
                else if (stoneState[_z, _x] == turn)
                {
                    // ひっくり返すことができる
                    isTurnable = true;
                    break;
                }
                else
                {
                    break;
                }
            }

            //ひっくり返す処理
            if (isTurnable)
            {
                count += turnableStoneList.Count;
                if (isTurn)
                {
                    for (int j = 0; j < turnableStoneList.Count; j++)
                    {
                        TurnableStone ts = turnableStoneList[j];
                        stoneState[ts.turnZ, ts.turnX] = turn;
                        turnCount++;
                        if (stoneState[ts.turnZ, ts.turnX] == eStoneState.WHITE) {
                            whiteScore ++;
                            blackScore --;
                        }
                        else if (stoneState[ts.turnZ, ts.turnX] == eStoneState.BLACK) {
                            blackScore ++;
                            whiteScore --;
                        }
                    }
                    seconds = 7;
                }
            }
        }
        return count;
    }

    //勝利判定
    private void CheckWinner(){
        if (blackScore + whiteScore == 64 || blackScore == 0 || whiteScore == 0) {
            if (blackScore > whiteScore) {
                blackWinText.SetActive(true);
            }
            else if (blackScore < whiteScore) {
                whiteWinText.SetActive(true);
            }
            timerText.text = "終了";
        }
    }

    public void AIPutStone(){
        //x,zの値を取得
        x = (int)UnityEngine.Random.Range(0, 8);
        z = (int)UnityEngine.Random.Range(0, 8);
        Debug.Log(x);
        Debug.Log(x);


        if (0 <= x && x < squareX && 0 <= z && z < squareZ &&
            stoneState[z, x] == eStoneState.EMPTY && Turn(false) > 0)
        {
            audioSource.PlayOneShot(putStoneSE);

            stoneState[z, x] = turn;
            if (turn == eStoneState.BLACK) {
                blackScore ++;
            }
            else if (turn == eStoneState.WHITE) {
                whiteScore ++;
            }
            Turn(true);
            
            turn = ((turn == eStoneState.BLACK) ? eStoneState.WHITE : eStoneState.BLACK);

            isAI = false;
        }
    }
    public void PushBackButton(){
        SceneManager.LoadScene("Title");

    }
}

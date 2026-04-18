using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    private const int DebuggerRoleId = 1;
    private const int HackerRoleId = 2;
    private const int CompilateurRoleId = 3;
    private const int CacheRoleId = 4;
    private const int DevOpsRoleId = 5;
    private const int MultiLanguageRoleId = 6;
    private const int FullstackRoleId = 9;
    private const int MaxCompilerHintsPerTheme = 5;
    private const int MaxHackerUsesPerTheme = 2;

    private int questionIndex = 0; //compteur séquentiel
    private bool waitingForNextQuestion = false;
    private IRoleEffect roleEffect;
    private int selectedRoleId;

    private int debuggerSkipsUsedThisTheme = 0;
    private int hackerUsesThisTheme = 0;
    private int fullstackSkipsUsedInRun = 0;
    private int compilerHintsUsedThisTheme = 0;
    private int multiLanguageUsesThisTheme = 0;
    private int devOpsUsesThisTheme = 0;
    private int cacheUsesThisTheme = 0;
    private bool compilerHintUsedOnCurrentQuestion = false;
    private bool devOpsQuestionUsedOnCurrentQuestion = false;
    private bool cacheQuestionUsedOnCurrentQuestion = false;
    private bool resumeDevOpsQuestionActive = false;
    private bool resumeCacheQuestionActive = false;

    public List<QuestionAndAnswer> QnA = new List<QuestionAndAnswer>();
    public GameObject[] options;
    public int currentQuestion;

    public GameObject Quizpanel;
    public GameObject NextPanel;

    [Header("Boss Question")]
    public Text bossWarningText;

    [Header("Panel + Themes")]
    public Text QuestionTxt;
    public Text ScoreTxt;
    public Text CurrentTheme;
    string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };

    [Header("Save System")]
    [SerializeField] public GameObject saveSlotsPanel;
    [SerializeField] public GameObject overwritePanel;
    [SerializeField] public Text overwriteText;
    private int pendingSlot = 1;

    [Header("Rôle")]
    [SerializeField] private Button skipButton;
    [SerializeField] private Text hintButtonText;
    [SerializeField] private Text hintDisplayText;
    [SerializeField] private Text playerRoleText;

    [Header("Progression")]
    [SerializeField] private Text questionNumberText;

    int totalQuestions = 0;
    int questionsAskedThisTheme = 0;

    string connStr = "Server=localhost;Database=quizgame;User ID=root;Password=rootroot;Port=3306;";
    private QuizRepository quizRepository;

    private void Start()
    {
        Debug.Log("[Quiz] Initialisation du gestionnaire de quiz.");

        CurrentTheme.text = "Theme : " + GameManager.Instance.themes[GameManager.Instance.currentThemeIndex];
        quizRepository = new QuizRepository(connStr);

        int resumeTheme = PlayerPrefs.GetInt("Resume_Theme", -1);
        int resumeQuestion = PlayerPrefs.GetInt("Resume_Question", 0);
        int resumeQuestionsAsked = PlayerPrefs.GetInt("Resume_QuestionsAsked", -1);
        string resumeQuestionOrder = PlayerPrefs.GetString("Resume_QuestionOrder", string.Empty);

        bool isResume = resumeTheme == GameManager.Instance.currentThemeIndex + 1;

        if (isResume)
        {
            questionIndex = resumeQuestion;
            questionsAskedThisTheme = resumeQuestionsAsked >= 0 ? resumeQuestionsAsked : resumeQuestion;
            debuggerSkipsUsedThisTheme = PlayerPrefs.GetInt("Resume_DebuggerSkipsUsedThisTheme", 0);
            hackerUsesThisTheme = PlayerPrefs.GetInt("Resume_HackerUsesThisTheme", 0);
            compilerHintsUsedThisTheme = PlayerPrefs.GetInt("Resume_CompilerHintsUsedThisTheme", 0);
            multiLanguageUsesThisTheme = PlayerPrefs.GetInt("Resume_MultiLanguageUsesThisTheme", 0);
            devOpsUsesThisTheme = PlayerPrefs.GetInt("Resume_DevOpsUsesThisTheme", 0);
            cacheUsesThisTheme = PlayerPrefs.GetInt("Resume_CacheUsesThisTheme", 0);
            resumeDevOpsQuestionActive = PlayerPrefs.GetInt("Resume_DevOpsQuestionActive", 0) == 1;
            resumeCacheQuestionActive = PlayerPrefs.GetInt("Resume_CacheQuestionActive", 0) == 1;
            fullstackSkipsUsedInRun = PlayerPrefs.GetInt("Resume_FullstackSkipsUsedInRun", PlayerPrefs.GetInt("Run_FullstackSkipsUsedInRun", 0));

            Debug.Log($"[Quiz] Reprise détectée: questionIndex={questionIndex}, questionsRépondues={questionsAskedThisTheme}.");
        }
        else
        {
            questionIndex = 0;
            questionsAskedThisTheme = 0;
            debuggerSkipsUsedThisTheme = 0;
            hackerUsesThisTheme = 0;
            compilerHintsUsedThisTheme = 0;
            multiLanguageUsesThisTheme = 0;
            devOpsUsesThisTheme = 0;
            cacheUsesThisTheme = 0;
            resumeDevOpsQuestionActive = false;
            resumeCacheQuestionActive = false;
            fullstackSkipsUsedInRun = PlayerPrefs.GetInt("Run_FullstackSkipsUsedInRun", 0);
            resumeQuestionOrder = string.Empty;
        }

        selectedRoleId = PlayerPrefs.GetInt("SelectedRoleId", -1);
        roleEffect = RoleEffectFactory.Create(selectedRoleId);

        ConfigureRoleUI();

        LoadQuestionsFromDatabase(resumeQuestionOrder);

        if (isResume && resumeDevOpsQuestionActive && resumeQuestion >= 0 && resumeQuestion < QnA.Count)
        {
            if (TryLoadDevOpsQuestionForCurrentTheme(out QuestionAndAnswer difficultQuestion))
            {
                difficultQuestion.Id = QnA[resumeQuestion].Id;
                difficultQuestion.IsDevOpsQuestion = true;
                difficultQuestion.IsCacheQuestion = false;
                QnA[resumeQuestion] = difficultQuestion;
            }
        }

        if (isResume && resumeCacheQuestionActive && resumeQuestion >= 0 && resumeQuestion < QnA.Count)
        {
            if (TryLoadCacheQuestionFromHistory(out QuestionAndAnswer cacheQuestion))
            {
                cacheQuestion.Id = QnA[resumeQuestion].Id;
                cacheQuestion.IsCacheQuestion = true;
                cacheQuestion.IsDevOpsQuestion = false;
                QnA[resumeQuestion] = cacheQuestion;
            }
        }

        totalQuestions = GameManager.Instance.questionPerTheme;
        CurrentTheme.text = "Theme : " + themes[GameManager.Instance.currentThemeIndex];
        NextPanel.SetActive(false);
        generateQuestion();

        PlayerPrefs.DeleteKey("Resume_Theme");
        PlayerPrefs.DeleteKey("Resume_Question");
        PlayerPrefs.DeleteKey("Resume_QuestionsAsked");
        PlayerPrefs.DeleteKey("Resume_Score");
        PlayerPrefs.DeleteKey("Resume_QuestionOrder");
        PlayerPrefs.DeleteKey("Resume_DebuggerSkipsUsedThisTheme");
        PlayerPrefs.DeleteKey("Resume_HackerUsesThisTheme");
        PlayerPrefs.DeleteKey("Resume_FullstackSkipsUsedInRun");
        PlayerPrefs.DeleteKey("Resume_CompilerHintsUsedThisTheme");
        PlayerPrefs.DeleteKey("Resume_MultiLanguageUsesThisTheme");
        PlayerPrefs.DeleteKey("Resume_DevOpsUsesThisTheme");
        PlayerPrefs.DeleteKey("Resume_DevOpsQuestionActive");
        PlayerPrefs.DeleteKey("Resume_CacheUsesThisTheme");
        PlayerPrefs.DeleteKey("Resume_CacheQuestionActive");

        if (bossWarningText != null)
            bossWarningText.gameObject.SetActive(false);
    }

    private void ConfigureRoleUI()
    {
        string selectedRoleName = PlayerPrefs.GetString("SelectedRoleName", "Aucun rôle");

        if (playerRoleText != null)
            playerRoleText.text = "Rôle : " + selectedRoleName;

        if (skipButton != null)
            skipButton.onClick.RemoveAllListeners();

        bool usesActionButton = roleEffect != null && (roleEffect.ShowsSkipButton || roleEffect.ShowsHintButton);

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(usesActionButton);

            if (usesActionButton)
            {
                if (roleEffect.ShowsHintButton)
                    skipButton.onClick.AddListener(UseHint);
                else
                    skipButton.onClick.AddListener(SkipQuestion);
            }
        }

        UpdateRoleActionButtonUI();
    }

    private void SetActionButtonLabel(string label)
    {
        if (skipButton != null)
        {
            Text legacyText = skipButton.GetComponentInChildren<Text>(true);
            if (legacyText != null)
                legacyText.text = label;

            TMP_Text tmpText = skipButton.GetComponentInChildren<TMP_Text>(true);
            if (tmpText != null)
                tmpText.text = label;
        }

        if (hintButtonText != null)
            hintButtonText.text = label;
    }

    private void UpdateRoleActionButtonUI()
    {
        if (skipButton == null)
            return;

        bool usesActionButton = roleEffect != null && (roleEffect.ShowsSkipButton || roleEffect.ShowsHintButton);
        if (!usesActionButton)
        {
            skipButton.gameObject.SetActive(false);
            return;
        }

        bool showButton = true;
        bool canUse = !waitingForNextQuestion;
        string buttonLabel = string.Empty;

        if (selectedRoleId == DebuggerRoleId)
        {
            int remaining = Mathf.Max(0, 1 - debuggerSkipsUsedThisTheme);
            showButton = remaining > 0;
            canUse = canUse && remaining > 0;
            buttonLabel = "Skip x" + remaining;
        }
        else if (selectedRoleId == HackerRoleId)
        {
            int remaining = Mathf.Max(0, MaxHackerUsesPerTheme - hackerUsesThisTheme);
            showButton = remaining > 0;
            canUse = canUse && remaining > 0;
            buttonLabel = "Hack x" + remaining;
        }
        else if (selectedRoleId == CacheRoleId)
        {
            int remaining = Mathf.Max(0, 1 - cacheUsesThisTheme);
            showButton = remaining > 0;
            canUse = canUse && remaining > 0;
            buttonLabel = "Cache x" + remaining;
        }
        else if (selectedRoleId == DevOpsRoleId)
        {
            int remaining = Mathf.Max(0, 1 - devOpsUsesThisTheme);
            showButton = remaining > 0;
            canUse = canUse && remaining > 0;
            buttonLabel = "DevOps x" + remaining;
        }
        else if (selectedRoleId == MultiLanguageRoleId)
        {
            int remaining = Mathf.Max(0, 1 - multiLanguageUsesThisTheme);
            showButton = remaining > 0;
            canUse = canUse && remaining > 0;
            buttonLabel = "Langue x" + remaining;
        }
        else if (selectedRoleId == FullstackRoleId)
        {
            int remaining = Mathf.Max(0, 1 - fullstackSkipsUsedInRun);
            showButton = remaining > 0;
            canUse = canUse && remaining > 0;
            buttonLabel = "Skip x" + remaining;
        }
        else if (selectedRoleId == CompilateurRoleId)
        {
            int remaining = Mathf.Max(0, MaxCompilerHintsPerTheme - compilerHintsUsedThisTheme);
            showButton = true;
            canUse = canUse && remaining > 0 && !compilerHintUsedOnCurrentQuestion;
            buttonLabel = "Indice x" + remaining;
        }

        skipButton.gameObject.SetActive(showButton);
        if (showButton)
            skipButton.interactable = canUse;

        SetActionButtonLabel(buttonLabel);
    }

    private void UpdateQuestionNumberUI()
    {
        if (questionNumberText == null)
            return;

        int maxQuestions = Mathf.Min(GameManager.Instance.questionPerTheme, 20);
        int currentDisplay = Mathf.Clamp(questionsAskedThisTheme + 1, 1, maxQuestions);
        questionNumberText.text = "Question " + currentDisplay + " / " + maxQuestions;
    }

    private void SkipQuestion()
    {
        roleEffect?.TryUseSkip(this);
    }

    private void UseHint()
    {
        roleEffect?.TryUseHint(this);
    }

    public bool TryUseDebuggerSkip()
    {
        if (debuggerSkipsUsedThisTheme >= 1)
            return false;

        if (!TrySkipQuestionWithPoints(1))
            return false;

        debuggerSkipsUsedThisTheme++;
        UpdateRoleActionButtonUI();
        return true;
    }

    public bool TryUseHackerHack()
    {
        if (selectedRoleId != HackerRoleId)
            return false;

        if (waitingForNextQuestion)
            return false;

        if (hackerUsesThisTheme >= MaxHackerUsesPerTheme)
            return false;

        if (currentQuestion < 0 || currentQuestion >= QnA.Count)
            return false;

        List<int> wrongIndexes = new List<int>();

        for (int i = 0; i < options.Length; i++)
        {
            if (QnA[currentQuestion].CorrectAnswer == i + 1)
                continue;

            Button optionButton = options[i].GetComponent<Button>();
            if (optionButton != null && optionButton.interactable)
                wrongIndexes.Add(i);
        }

        if (wrongIndexes.Count < 2)
            return false;

        for (int n = 0; n < 2; n++)
        {
            int pick = UnityEngine.Random.Range(0, wrongIndexes.Count);
            int optionIndex = wrongIndexes[pick];
            wrongIndexes.RemoveAt(pick);

            Button optionButton = options[optionIndex].GetComponent<Button>();
            if (optionButton != null)
                optionButton.interactable = false;

            Image img = options[optionIndex].GetComponent<Image>();
            if (img != null)
                img.color = Color.gray;
        }

        hackerUsesThisTheme++;
        UpdateRoleActionButtonUI();
        return true;
    }

    public bool TryUseCacheReplay()
    {
        if (selectedRoleId != CacheRoleId)
            return false;

        if (waitingForNextQuestion)
            return false;

        if (cacheUsesThisTheme >= 1)
            return false;

        if (currentQuestion < 0 || currentQuestion >= QnA.Count)
            return false;

        if (!TryLoadCacheQuestionFromHistory(out QuestionAndAnswer cachedQuestion))
            return false;

        int originalId = QnA[currentQuestion].Id;
        cachedQuestion.Id = originalId;
        cachedQuestion.IsCacheQuestion = true;
        cachedQuestion.IsDevOpsQuestion = false;
        QnA[currentQuestion] = cachedQuestion;

        QuestionTxt.text = QnA[currentQuestion].Question;
        SetAnswers();

        if (hintDisplayText != null)
            hintDisplayText.text = string.Empty;

        compilerHintUsedOnCurrentQuestion = false;
        devOpsQuestionUsedOnCurrentQuestion = false;
        cacheQuestionUsedOnCurrentQuestion = true;
        cacheUsesThisTheme++;
        UpdateRoleActionButtonUI();

        Debug.Log("[Quiz][Rôle Cache] Question historique rejouée.");
        return true;
    }

    private bool TryLoadCacheQuestionFromHistory(out QuestionAndAnswer cachedQuestion)
    {
        cachedQuestion = null;

        if (!GameManager.Instance.TryGetRandomAnsweredQuestion(out QuestionAndAnswer previousQuestion))
            return false;

        if (previousQuestion == null || previousQuestion.Answers == null || previousQuestion.Answers.Length < 4)
            return false;

        string[] answers = new string[4]
        {
            previousQuestion.Answers[0],
            previousQuestion.Answers[1],
            previousQuestion.Answers[2],
            previousQuestion.Answers[3]
        };

        cachedQuestion = new QuestionAndAnswer
        {
            Question = previousQuestion.Question,
            Answers = answers,
            CorrectAnswer = previousQuestion.CorrectAnswer,
            Hint = previousQuestion.Hint,
            IsCacheQuestion = true,
            IsDevOpsQuestion = false
        };

        return true;
    }

    public bool TryUseDevOpsSwap()
    {
        if (selectedRoleId != DevOpsRoleId)
            return false;

        if (waitingForNextQuestion)
            return false;

        if (devOpsUsesThisTheme >= 1)
            return false;

        if (currentQuestion < 0 || currentQuestion >= QnA.Count)
            return false;

        if (!TryLoadDevOpsQuestionForCurrentTheme(out QuestionAndAnswer difficultQuestion))
            return false;

        int originalId = QnA[currentQuestion].Id;
        difficultQuestion.Id = originalId;
        difficultQuestion.IsDevOpsQuestion = true;
        difficultQuestion.IsCacheQuestion = false;
        QnA[currentQuestion] = difficultQuestion;

        QuestionTxt.text = QnA[currentQuestion].Question;
        SetAnswers();

        if (hintDisplayText != null)
            hintDisplayText.text = string.Empty;

        compilerHintUsedOnCurrentQuestion = false;
        devOpsQuestionUsedOnCurrentQuestion = true;
        cacheQuestionUsedOnCurrentQuestion = false;
        devOpsUsesThisTheme++;
        UpdateRoleActionButtonUI();

        Debug.Log("[Quiz][Rôle DevOps] Question remplacée par une question difficile.");
        return true;
    }

    private bool TryLoadDevOpsQuestionForCurrentTheme(out QuestionAndAnswer difficultQuestion)
    {
        difficultQuestion = null;
        int themeId = GameManager.Instance.currentThemeIndex + 1;

        try
        {
            difficultQuestion = quizRepository.LoadDevOpsQuestionByTheme(themeId);
            if (difficultQuestion == null)
                return false;

            difficultQuestion.Hint = string.Empty;
            difficultQuestion.IsDevOpsQuestion = true;
            difficultQuestion.IsCacheQuestion = false;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("[Quiz][Rôle DevOps] Échec du chargement de la question difficile : " + ex.Message);
            return false;
        }
    }

    public bool TryUseFullstackSkip()
    {
        if (fullstackSkipsUsedInRun >= 1)
            return false;

        if (!TrySkipQuestionWithPoints(2))
            return false;

        fullstackSkipsUsedInRun++;
        PlayerPrefs.SetInt("Run_FullstackSkipsUsedInRun", fullstackSkipsUsedInRun);
        UpdateRoleActionButtonUI();
        return true;
    }

    public bool TryUseCompilerHint()
    {
        if (selectedRoleId != CompilateurRoleId)
            return false;

        if (waitingForNextQuestion)
            return false;

        if (currentQuestion < 0 || currentQuestion >= QnA.Count)
            return false;

        if (compilerHintsUsedThisTheme >= MaxCompilerHintsPerTheme)
            return false;

        if (compilerHintUsedOnCurrentQuestion)
            return false;

        string hint = QnA[currentQuestion].Hint;
        if (string.IsNullOrEmpty(hint))
            hint = "Aucun indice disponible.";

        if (hintDisplayText != null)
            hintDisplayText.text = "Indice : " + hint;
        else
            QuestionTxt.text = QnA[currentQuestion].Question + "\nIndice : " + hint;

        compilerHintsUsedThisTheme++;
        compilerHintUsedOnCurrentQuestion = true;
        UpdateRoleActionButtonUI();
        return true;
    }

    public bool TrySkipQuestionWithPoints(int points)
    {
        if (waitingForNextQuestion)
            return false;

        if (currentQuestion < 0 || currentQuestion >= QnA.Count)
            return false;

        GameManager.Instance.RegisterAnsweredQuestion(QnA[currentQuestion]);
        GameManager.Instance.AddPointsToCurrentTheme(points);

        questionsAskedThisTheme++;
        waitingForNextQuestion = true;

        Debug.Log($"[Quiz][Action] Question passée avec bonus de {points} point(s).");
        UpdateRoleActionButtonUI();
        StartCoroutine(WaitForNext());
        return true;
    }

    void LoadQuestionsFromDatabase(string forcedQuestionOrder)
    {
        QnA.Clear();

        try
        {
            int themeId = GameManager.Instance.currentThemeIndex + 1;
            int questionsToLoad = Mathf.Min(GameManager.Instance.questionPerTheme, 20);

            List<int> forcedIds = ParseQuestionOrder(forcedQuestionOrder);
            if (forcedIds.Count > 0)
            {
                bool exactLoaded = TryLoadQuestionsInExactOrder(forcedIds);
                if (exactLoaded)
                {
                    Debug.Log($"[Quiz] Reprise exacte réussie: {QnA.Count} question(s) restaurée(s) pour le thème {themeId}.");
                    return;
                }

                Debug.LogWarning("[Quiz] Impossible de restaurer l'ordre sauvegardé. Chargement aléatoire appliqué.");
                QnA.Clear();
            }

            string currentTheme = themes[GameManager.Instance.currentThemeIndex];
            List<QuestionAndAnswer> loadedQuestions = quizRepository.LoadQuestionsByTheme(themeId, currentTheme);

            if (loadedQuestions.Count == 0)
            {
                Debug.LogError("[Quiz] Aucune question récupérée depuis la base (schéma ou données à vérifier).");
                return;
            }

            for (int i = loadedQuestions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                QuestionAndAnswer temp = loadedQuestions[i];
                loadedQuestions[i] = loadedQuestions[j];
                loadedQuestions[j] = temp;
            }

            int count = Mathf.Min(questionsToLoad, loadedQuestions.Count);
            for (int i = 0; i < count; i++)
                QnA.Add(loadedQuestions[i]);

            Debug.Log($"[Quiz] Thème {themeId} chargé: {QnA.Count}/{questionsToLoad} question(s) prêtes.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[Quiz] Erreur d'accès base de données : " + ex.Message);
        }
    }

    private List<int> ParseQuestionOrder(string raw)
    {
        List<int> ids = new List<int>();
        if (string.IsNullOrWhiteSpace(raw))
            return ids;

        string[] parts = raw.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int id) && id > 0)
                ids.Add(id);
        }

        return ids;
    }

    private string BuildCurrentQuestionOrder()
    {
        List<string> ids = new List<string>();
        for (int i = 0; i < QnA.Count; i++)
        {
            if (QnA[i] != null && QnA[i].Id > 0)
                ids.Add(QnA[i].Id.ToString());
        }

        return string.Join(",", ids);
    }

    private bool TryLoadQuestionsInExactOrder(List<int> orderedIds)
    {
        List<QuestionAndAnswer> orderedQuestions = quizRepository.LoadQuestionsByIdsInOrder(orderedIds);
        if (orderedQuestions.Count != orderedIds.Count)
            return false;

        QnA.AddRange(orderedQuestions);
        return true;
    }

    bool IsBossQuestion()
    {
        int remaining = GameManager.Instance.questionPerTheme - questionsAskedThisTheme;
        return remaining <= 3;
    }

    public void SauvegarderDansSlot(int slot)
    {
        int indexToSave = waitingForNextQuestion ? questionIndex : currentQuestion;
        string prefix = "Save" + slot + "_";

        Debug.Log($">>> [Sauvegarder] slot={slot}, currentQuestion={currentQuestion}, indexToSave={indexToSave} <<<");

        PlayerPrefs.SetString(prefix + "PlayerName", PlayerPrefs.GetString("PlayerName", "Inconnu"));
        PlayerPrefs.SetInt(prefix + "Theme", GameManager.Instance.currentThemeIndex + 1);
        PlayerPrefs.SetInt(prefix + "Question", indexToSave);
        PlayerPrefs.SetInt(prefix + "QuestionsAsked", questionsAskedThisTheme);
        PlayerPrefs.SetInt(prefix + "RoleId", PlayerPrefs.GetInt("SelectedRoleId", -1));
        PlayerPrefs.SetString(prefix + "RoleName", PlayerPrefs.GetString("SelectedRoleName", "Aucun rôle"));
        PlayerPrefs.SetString(prefix + "QuestionOrder", BuildCurrentQuestionOrder());
        PlayerPrefs.SetInt(prefix + "DebuggerSkipsUsedThisTheme", debuggerSkipsUsedThisTheme);
        PlayerPrefs.SetInt(prefix + "HackerUsesThisTheme", hackerUsesThisTheme);
        PlayerPrefs.SetInt(prefix + "CacheUsesThisTheme", cacheUsesThisTheme);
        PlayerPrefs.SetInt(prefix + "CacheQuestionActive", currentQuestion >= 0 && currentQuestion < QnA.Count && QnA[currentQuestion].IsCacheQuestion ? 1 : 0);
        PlayerPrefs.SetInt(prefix + "DevOpsUsesThisTheme", devOpsUsesThisTheme);
        PlayerPrefs.SetInt(prefix + "DevOpsQuestionActive", currentQuestion >= 0 && currentQuestion < QnA.Count && QnA[currentQuestion].IsDevOpsQuestion ? 1 : 0);
        PlayerPrefs.SetInt(prefix + "FullstackSkipsUsedInRun", fullstackSkipsUsedInRun);
        PlayerPrefs.SetInt(prefix + "CompilerHintsUsedThisTheme", compilerHintsUsedThisTheme);
        PlayerPrefs.SetInt(prefix + "MultiLanguageUsesThisTheme", multiLanguageUsesThisTheme);

        for (int i = 0; i < 5; i++)
            PlayerPrefs.SetInt(prefix + "ScoreTheme" + i, GameManager.Instance.themeScores[i]);

        PlayerPrefs.Save();

        Debug.Log($"[Quiz][Save] Sauvegarde effectuée dans le slot {slot} (theme={GameManager.Instance.currentThemeIndex + 1}, question={indexToSave}).");
    }

    public void OuvrirChoixSlot()
    {
        saveSlotsPanel.SetActive(true);
    }

    public void FermerChoixSlot()
    {
        saveSlotsPanel.SetActive(false);
        overwritePanel.SetActive(false);
    }

    public void ChoisirSlot(int slot)
    {
        pendingSlot = slot;

        string prefix = "Save" + slot + "_";
        bool existe = PlayerPrefs.HasKey(prefix + "PlayerName");

        if (existe)
        {
            overwriteText.text = "Écraser la sauvegarde de l'emplacement " + slot + " ?";
            overwritePanel.SetActive(true);
        }
        else
        {
            SauvegarderDansSlot(slot);
            saveSlotsPanel.SetActive(false);
        }
    }

    public void ConfirmerOverwriteOui()
    {
        SauvegarderDansSlot(pendingSlot);
        overwritePanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
    }

    public void ConfirmerOverwriteNon()
    {
        overwritePanel.SetActive(false);
    }

    public void retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        Quizpanel.SetActive(false);
        NextPanel.SetActive(true);

        int themeScore = GameManager.Instance.themeScores[GameManager.Instance.currentThemeIndex];
        ScoreTxt.text = "Score du thème : " + themeScore + " / " + GameManager.Instance.questionPerTheme;
    }

    public void SkipThemeForDebug()
    {
        waitingForNextQuestion = false;
        questionsAskedThisTheme = GameManager.Instance.questionPerTheme;
        questionIndex = QnA.Count;

        Debug.LogWarning($"[Quiz][Debug] Thème {GameManager.Instance.currentThemeIndex + 1} ignoré (sans points ni historique).");
        GameOver();
    }

    public void correct()
    {
        if (waitingForNextQuestion)
            return;

        if (currentQuestion >= 0 && currentQuestion < QnA.Count)
            GameManager.Instance.RegisterAnsweredQuestion(QnA[currentQuestion]);

        if (QnA[currentQuestion].IsCacheQuestion)
        {
            GameManager.Instance.AddPointToCurrentTheme();
        }
        else if (QnA[currentQuestion].IsDevOpsQuestion)
        {
            GameManager.Instance.AddPointsToCurrentTheme(3);
        }
        else
        {
            int remaining = GameManager.Instance.questionPerTheme - questionsAskedThisTheme;

            if (remaining <= 3)
            {
                GameManager.Instance.AddPointsToCurrentTheme(2);
            }
            else
            {
                GameManager.Instance.AddPointToCurrentTheme();
            }
        }

        questionsAskedThisTheme++;
        waitingForNextQuestion = true;
        UpdateRoleActionButtonUI();

        StartCoroutine(WaitForNext());
    }

    public void wrong()
    {
        if (waitingForNextQuestion)
            return;

        if (currentQuestion >= 0 && currentQuestion < QnA.Count)
            GameManager.Instance.RegisterAnsweredQuestion(QnA[currentQuestion]);

        if (QnA[currentQuestion].IsDevOpsQuestion)
        {
            GameManager.Instance.AddPointsToCurrentTheme(-1);
        }

        questionsAskedThisTheme++;
        waitingForNextQuestion = true;
        UpdateRoleActionButtonUI();

        StartCoroutine(WaitForNext());
    }

    IEnumerator WaitForNext()
    {
        yield return new WaitForSeconds(1);
        generateQuestion();
    }

    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<AnswerScript>().isCorrect = false;

            Button optionButton = options[i].GetComponent<Button>();
            if (optionButton != null)
                optionButton.interactable = true;

            options[i].transform.GetChild(0).GetComponent<Text>().text = QnA[currentQuestion].Answers[i];
            options[i].GetComponent<Image>().color = options[i].GetComponent<AnswerScript>().startColor;

            if (QnA[currentQuestion].CorrectAnswer == i + 1)
            {
                options[i].GetComponent<AnswerScript>().isCorrect = true;
            }

            options[i].GetComponent<Image>().color = options[i].GetComponent<AnswerScript>().startColor;
        }
    }

    void generateQuestion()
    {
        waitingForNextQuestion = false;

        if (questionIndex >= QnA.Count || questionsAskedThisTheme >= GameManager.Instance.questionPerTheme)
        {
            Debug.Log($"[Quiz] Fin du thème {GameManager.Instance.currentThemeIndex + 1}.");
            GameOver();
            return;
        }

        currentQuestion = questionIndex;
        compilerHintUsedOnCurrentQuestion = false;
        devOpsQuestionUsedOnCurrentQuestion = QnA[currentQuestion].IsDevOpsQuestion;
        cacheQuestionUsedOnCurrentQuestion = QnA[currentQuestion].IsCacheQuestion;
        UpdateQuestionNumberUI();

        if (hintDisplayText != null)
            hintDisplayText.text = string.Empty;

        UpdateRoleActionButtonUI();
        QuestionTxt.text = QnA[currentQuestion].Question;
        SetAnswers();

        if (bossWarningText != null)
            bossWarningText.gameObject.SetActive(IsBossQuestion());

        questionIndex++;
    }

    public void NextTheme()
    {
        debuggerSkipsUsedThisTheme = 0;
        hackerUsesThisTheme = 0;
        cacheUsesThisTheme = 0;
        compilerHintsUsedThisTheme = 0;
        multiLanguageUsesThisTheme = 0;
        devOpsUsesThisTheme = 0;
        devOpsQuestionUsedOnCurrentQuestion = false;
        cacheQuestionUsedOnCurrentQuestion = false;

        GameManager.Instance.currentThemeIndex++;

        if (GameManager.Instance.currentThemeIndex < 5)
        {
            string nextSceneName = "Theme" + (GameManager.Instance.currentThemeIndex + 1);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene("EndScene");
        }
    }

    public int QuestionIndex
    {
        get { return questionIndex; }
    }

    public bool TryUseMultiLanguageSwap()
    {
        if (selectedRoleId != MultiLanguageRoleId)
            return false;

        if (waitingForNextQuestion)
            return false;

        if (multiLanguageUsesThisTheme >= 1)
            return false;

        if (currentQuestion < 0 || currentQuestion >= QnA.Count)
            return false;

        if (!TryLoadSecoursQuestionForCurrentTheme(out QuestionAndAnswer secoursQuestion))
            return false;

        int originalId = QnA[currentQuestion].Id;
        secoursQuestion.Id = originalId;
        secoursQuestion.IsDevOpsQuestion = false;
        secoursQuestion.IsCacheQuestion = false;
        QnA[currentQuestion] = secoursQuestion;

        QuestionTxt.text = QnA[currentQuestion].Question;
        SetAnswers();

        if (hintDisplayText != null)
            hintDisplayText.text = string.Empty;

        compilerHintUsedOnCurrentQuestion = false;
        devOpsQuestionUsedOnCurrentQuestion = false;
        cacheQuestionUsedOnCurrentQuestion = false;
        multiLanguageUsesThisTheme++;
        UpdateRoleActionButtonUI();

        Debug.Log("[Quiz][Rôle Multi Language] Question remplacée par une question de secours.");
        return true;
    }

    private bool TryLoadSecoursQuestionForCurrentTheme(out QuestionAndAnswer secoursQuestion)
    {
        secoursQuestion = null;
        int themeId = GameManager.Instance.currentThemeIndex + 1;

        try
        {
            secoursQuestion = quizRepository.LoadSecoursQuestionByTheme(themeId);
            if (secoursQuestion == null)
                return false;

            secoursQuestion.Hint = string.Empty;
            secoursQuestion.IsDevOpsQuestion = false;
            secoursQuestion.IsCacheQuestion = false;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("[Quiz][Rôle Multi Language] Échec du chargement de la question de secours : " + ex.Message);
            return false;
        }
    }
}

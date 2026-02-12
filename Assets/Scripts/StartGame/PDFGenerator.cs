using UnityEngine;
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

// Alias pour éviter les conflits de noms
using iTextFont = iTextSharp.text.Font;
using iTextImage = iTextSharp.text.Image;

public class PDFGenerator : MonoBehaviour
{

    public string[] themes = { "Culture générale", "Musique", "Cinéma", "Sport", "Géographie" };

    public static void GenerateScorePDF(string playerName, int totalScore, int[] themeScores, string[] themes)
    {
        string fileName = "QuizScore_" + playerName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".pdf";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        Document document = new Document(PageSize.A4, 50, 50, 25, 25);

        try
        {
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));
            document.Open();

            // === Utilise iTextFont au lieu de Font ===
            iTextFont titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
            iTextFont normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
            iTextFont boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);

            // Titre
            Paragraph title = new Paragraph("Résultats du Quiz - Culture Générale", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            document.Add(title);

            // Présentation du jeu
            Paragraph intro = new Paragraph(
                "Ce quiz teste vos connaissances à travers 5 thèmes variés : Culture générale, Musique, Cinéma, Sport et Géographie. " +
                "Chaque thème comporte 20 questions pour un total de 100 points.",
                normalFont
            );
            intro.Alignment = Element.ALIGN_JUSTIFIED;
            intro.SpacingAfter = 15f;
            document.Add(intro);

            // Ligne de séparation
            document.Add(new Paragraph("_____________________________________________"));
            document.Add(Chunk.NEWLINE);

            // === Informations du joueur ===
            document.Add(new Paragraph("Joueur : " + playerName, boldFont));
            document.Add(new Paragraph("Date et heure : " + DateTime.Now.ToString("dd/MM/yyyy à HH:mm"), normalFont));
            document.Add(Chunk.NEWLINE);

            // === Score total ===
            Paragraph scoreTotal = new Paragraph("Score Total : " + totalScore + " / 100", titleFont);
            scoreTotal.Alignment = Element.ALIGN_CENTER;
            scoreTotal.SpacingAfter = 20f;
            document.Add(scoreTotal);

            // === Détail par thème ===
            document.Add(new Paragraph("Détail par thème :", boldFont));
            document.Add(Chunk.NEWLINE);

            // Création d'un tableau pour les scores
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 80;
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;

            // En-têtes du tableau
            PdfPCell headerTheme = new PdfPCell(new Phrase("Thème", boldFont));
            headerTheme.BackgroundColor = BaseColor.LIGHT_GRAY;
            headerTheme.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(headerTheme);

            PdfPCell headerScore = new PdfPCell(new Phrase("Score", boldFont));
            headerScore.BackgroundColor = BaseColor.LIGHT_GRAY;
            headerScore.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(headerScore);

            // Lignes de scores
            for (int i = 0; i < themes.Length; i++)
            {
                PdfPCell themeCell = new PdfPCell(new Phrase(themes[i], normalFont));
                themeCell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(themeCell);

                PdfPCell scoreCell = new PdfPCell(new Phrase(themeScores[i] + " / 20", normalFont));
                scoreCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(scoreCell);
            }

            document.Add(table);

            // === Message de félicitations ===
            document.Add(Chunk.NEWLINE);
            string message = totalScore >= 80 ? "Félicitations ! Excellent score !" :
                           totalScore >= 60 ? "Bon travail, continuez ainsi !" :
                           totalScore >= 40 ? "Pas mal, mais il y a de la marge de progression !" :
                           "Ne vous découragez pas, réessayez pour vous améliorer !";

            Paragraph congrats = new Paragraph(message, boldFont);
            congrats.Alignment = Element.ALIGN_CENTER;
            document.Add(congrats);

            document.Close();
            writer.Close();

            Debug.Log("PDF généré avec succès : " + path);
            Application.OpenURL("file://" + path);
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la génération du PDF : " + e.Message);
        }
    }
}

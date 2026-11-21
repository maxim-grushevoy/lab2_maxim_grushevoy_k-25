<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes"/>

  <!-- Українські назви атрибутів -->
  <xsl:template name="label">
    <xsl:param name="n"/>
    <xsl:choose>
      <xsl:when test="$n='author'">Автор</xsl:when>
      <xsl:when test="$n='title'">Назва</xsl:when>
      <xsl:when test="$n='annotation'">Анотація</xsl:when>
      <xsl:when test="$n='qualification'">Кваліфікаційна ознака</xsl:when>
      <xsl:when test="$n='year'">Рік</xsl:when>
      <xsl:when test="$n='isbn'">ISBN</xsl:when>
      <xsl:when test="$n='fio'">П.І.П.</xsl:when>
      <xsl:when test="$n='faculty'">Факультет</xsl:when>
      <xsl:when test="$n='department'">Кафедра</xsl:when>
      <xsl:when test="$n='position'">Посада</xsl:when>
      <xsl:otherwise><xsl:value-of select="$n"/></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/library">
    <html>
      <head>
        <meta charset="utf-8"/>
        <title>Кафедральна бібліотека</title>
        <style>
          body{font-family:Segoe UI,Arial,sans-serif;margin:16px;background:#fafafa}
          h1{font-size:20px}
          .book{background:#fff;border:1px solid #ccc;border-radius:6px;padding:10px;margin-bottom:12px}
          .book h2{font-size:16px;margin:0 0 6px 0;color:#1a4d8f}
          table{border-collapse:collapse;width:100%;margin-top:6px}
          th,td{border:1px solid #bbb;padding:4px 8px;text-align:left;font-size:13px}
          th{background:#e8eef7}
        </style>
      </head>
      <body>
        <h1>Кафедральна бібліотека — результати пошуку</h1>
        <p>Знайдено видань: <b><xsl:value-of select="count(book)"/></b></p>
        <xsl:apply-templates select="book"/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="book">
    <div class="book">
      <h2>
        <xsl:value-of select="position()"/>.
        <xsl:value-of select="@title"/>
      </h2>
      <xsl:for-each select="@*[name()!='title']">
        <div>
          <b><xsl:call-template name="label"><xsl:with-param name="n" select="name()"/></xsl:call-template>:</b>
          <xsl:text> </xsl:text><xsl:value-of select="."/>
        </div>
      </xsl:for-each>
      <xsl:if test="reader">
        <table>
          <tr>
            <th>П.І.П. читача</th><th>Факультет</th><th>Кафедра</th><th>Посада</th>
          </tr>
          <xsl:for-each select="reader">
            <tr>
              <td><xsl:value-of select="@fio"/></td>
              <td><xsl:value-of select="@faculty"/></td>
              <td><xsl:value-of select="@department"/></td>
              <td><xsl:value-of select="@position"/></td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
    </div>
  </xsl:template>
</xsl:stylesheet>

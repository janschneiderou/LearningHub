<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="Applications">
    <Applications>
      <xsl:apply-templates/>
    </Applications>
  </xsl:template>

  <xsl:template match="Application">
    <Application>
      <xsl:for-each select="*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="text()"/>
        </xsl:attribute>
      </xsl:for-each>
    </Application>
  </xsl:template>
</xsl:stylesheet>

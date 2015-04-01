<?xml version="1.0" encoding="UTF-8"?>
<?altova_samplexml file:///C:/Users/Tor%20Kjetil/Dropbox/Statkart/ShapeChange/SOSI_EA/Svalbardplan/objektkatalog/Svalbardplan%204.5.xml?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:gco="http://www.isotc211.org/2005/gco" xmlns:gfc="http://www.isotc211.org/2005/gfc/1.1" xmlns:gmd="http://www.isotc211.org/2005/gmd" xmlns:gmx="http://www.isotc211.org/2005/gmx" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<xsl:output method="html" indent="no"/>
	<xsl:template match="/">
		<html>
			<head>
				<title>Objektkatalog <xsl:value-of select="gfc:FC_FeatureCatalogue/gmx:name/gco:CharacterString" disable-output-escaping="yes"/>
				</title>
				<style type="text/css">
        body {
          background-color:white; 
          font-family: Sans-serif; 
          font-size: 0.8em;
        }
        h1 { font-size: 2em; }
        h2 { font-size: 1.3em; }
        h2 a { color: darkblue; }
        .feature { 
          margin-bottom: 20px;
          padding: 0px 10px 10px 10px;
        }
        .property { 
          zoom: 1;
          overflow: auto; 
          clear: both;
        }

        .property .name { 
          font-weight: bold; 
          float:left; 
          width: 100px;
        }
        .property .value { margin-left: 20px; }
        .property .value ul { 
          clear: both; 
          margin-left: 100px; 
        }
        .box { 
          border: 1px solid gray; 
          margin: 10px 0;
          padding: 5px;
        }
        ul { margin-top: 0px; }

        .footer p { text-align: center; font-size: 0.8em; }
      </style>
			</head>
			<body>
				<h1>
      Objektkatalog 
      <xsl:value-of select="gfc:FC_FeatureCatalogue/gmx:name/gco:CharacterString" disable-output-escaping="yes"/>
				</h1>
				<div class="property">
					<div class="name">Versjon</div>
					<div class="value">
						<xsl:value-of select="gfc:FC_FeatureCatalogue/gmx:versionNumber/gco:CharacterString" disable-output-escaping="yes"/>
					</div>
				</div>
				<div class="property">
					<div class="name">Dato</div>
					<div class="value">
						<xsl:value-of select="gfc:FC_FeatureCatalogue/gmx:versionDate/gco:Date" disable-output-escaping="yes"/>
					</div>
				</div>
				<xsl:if test="gfc:FC_FeatureCatalogue/gmx:scope">
					<div class="property">
						<div class="name">Beskrivelse</div>
						<xsl:for-each select="gfc:FC_FeatureCatalogue/gmx:scope/gco:CharacterString">
							<div class="value">
								<xsl:value-of select="." disable-output-escaping="yes"/>
							</div>
						</xsl:for-each>
					</div>
				</xsl:if>
				<div class="property">
					<div class="name">Ansvarlig</div>
					<div class="value">
						<xsl:variable name="responsible" select="gfc:FC_FeatureCatalogue/gfc:producer/gmd:CI_ResponsibleParty"/>
						<xsl:if test="$responsible/gmd:individualName">
							<div class="property">
								<div class="name">Navn</div>
								<div class="value">
									<xsl:value-of select="$responsible/gmd:individualName/gco:CharacterString" disable-output-escaping="yes"/>
								</div>
							</div>
						</xsl:if>
						<xsl:if test="$responsible/gmd:organisationName">
							<div class="property">
								<div class="name">Organisasjon</div>
								<div class="value">
									<xsl:value-of select="$responsible/gmd:organisationName/gco:CharacterString" disable-output-escaping="yes"/>
								</div>
							</div>
						</xsl:if>
						<xsl:if test="$responsible/gmd:positionName">
							<div class="property">
								<div class="name">Posisjon</div>
								<div class="value">
									<xsl:value-of select="$responsible/gmd:positionName/gco:CharacterString" disable-output-escaping="yes"/>
								</div>
							</div>
						</xsl:if>
						<xsl:if test="$responsible/gmd:role">
							<div class="property">
								<div class="name">Rolle</div>
								<div class="value">
									<a>
										<xsl:attribute name="href">
											<xsl:value-of select="$responsible/gmd:role/gmd:CI_RoleCode/@codeList"/>
										</xsl:attribute>
										<xsl:value-of select="$responsible/gmd:role/gmd:CI_RoleCode" disable-output-escaping="yes"/>
									</a>
								</div>
							</div>
						</xsl:if>
					</div>
				</div>
				<h2>Innhold</h2>
				<ul>
					<xsl:for-each select="gfc:FC_FeatureCatalogue/gfc:featureType/gfc:FC_FeatureType">
						<xsl:apply-templates select="." mode="overview"/>
					</xsl:for-each>
				</ul>
				<xsl:for-each select="gfc:FC_FeatureCatalogue/gfc:featureType/gfc:FC_FeatureType">
					<xsl:apply-templates select="." mode="details"/>
				</xsl:for-each>
				<hr/>
				<div class="footer">
					<p>
        Objektkatalog fra <a href="http://www.kartverket.no/Standarder/SOSI/Retningslinjer-og-veiledere-SOSI/">EA SOSI-plugin</a>
					</p>
				</div>
			</body>
		</html>
	</xsl:template>
	<xsl:template match="gfc:FC_FeatureType" mode="overview">
		<xsl:variable name="featuretype" select="."/>
		<li>
			<a>
				<xsl:attribute name="href">#<xsl:value-of select="$featuretype/@id"/>
				</xsl:attribute>
				<xsl:value-of select="$featuretype/gfc:typeName/gco:LocalName"/>
			</a>
		</li>
	</xsl:template>
	<xsl:template match="gfc:FC_FeatureType" mode="details">
		<xsl:variable name="featuretype" select="."/>
		<div class="feature box">
			<h2 class="name">Objekttype: 
      <a>
					<xsl:attribute name="id">
						<xsl:value-of select="$featuretype/@id"/>
					</xsl:attribute>
					<xsl:value-of select="$featuretype/gfc:typeName/gco:LocalName"/>
				</a>
			</h2>
			<xsl:if test="$featuretype/gfc:definition/gco:CharacterString  != ''">
				<div class="property">
					<div class="name">Definisjon</div>
					<div class="value">
						<xsl:value-of select="$featuretype/gfc:definition/gco:CharacterString"/>
					</div>
				</div>
			</xsl:if>
			<xsl:if test="$featuretype/gfc:isAbstract/gco:Boolean  != 'false'">
				<div class="property">
					<div class="name">Abstrakt</div>
					<div class="value">
						<xsl:value-of select="$featuretype/gfc:isAbstract/gco:Boolean"/>
					</div>
				</div>
			</xsl:if>
			<xsl:if test="$featuretype/gfc:aliases">
				<div class="property">
					<div class="name">Alias</div>
					<div class="value">
						<xsl:value-of select="$featuretype/gfc:aliases/gco:LocalName"/>
					</div>
				</div>
			</xsl:if>
			<div class="property characteristics">
				<div class="name">Egenskaper</div>
				<div class="value">
					<xsl:for-each select="$featuretype/gfc:carrierOfCharacteristics">
						<xsl:apply-templates select="."/>
					</xsl:for-each>
				</div>
			</div>
			<xsl:if test="gfc:constrainedBy">
				<div class="property constraints">
					<div class="name">Restriksjoner</div>
					<div class="value">
						<ul>
							<xsl:for-each select="$featuretype/gfc:constrainedBy">
								<xsl:apply-templates select="."/>
							</xsl:for-each>
						</ul>
					</div>
				</div>
			</xsl:if>
		</div>
	</xsl:template>
	<xsl:template match="gfc:carrierOfCharacteristics">
		<xsl:apply-templates select="gfc:FC_FeatureAttribute"/>
	</xsl:template>
	<xsl:template match="gfc:FC_FeatureAttribute">
		<div class="property attributes box">
			<div class="name">Egenskap</div>
			<div class="value">
				<div class="property">
					<div class="name">Navn</div>
					<div class="value">
						<xsl:value-of select="gfc:memberName/gco:LocalName"/>
					</div>
				</div>
				<xsl:if test="gfc:definition">
					<div class="property">
						<div class="name">Definisjon</div>
						<div class="value">
							<xsl:value-of select="gfc:definition/gco:CharacterString"/>
						</div>
					</div>
				</xsl:if>
				<xsl:apply-templates select="gfc:cardinality/gco:Multiplicity"/>
				<xsl:if test="gfc:valueType">
					<div class="property">
						<div class="name">Datatype</div>
						<div class="value">
							<xsl:value-of select="gfc:valueType/gco:TypeName/gco:aName/gco:CharacterString"/>
						</div>
					</div>
				</xsl:if>
				<xsl:if test="gfc:listedValue">
					<div class="property">
						<div class="name">Listeverdier</div>
						<div class="value">
							<ul>
								<xsl:apply-templates select="gfc:listedValue/gfc:FC_ListedValue"/>
							</ul>
						</div>
					</div>
				</xsl:if>
			</div>
		</div>
	</xsl:template>
	<xsl:template match="gfc:cardinality/gco:Multiplicity">
		<xsl:variable name="range" select="gco:range/gco:MultiplicityRange"/>
		<div class="property">
			<div class="name">Kardinalitet</div>
			<div class="value">
				<xsl:value-of select="$range/gco:lower/gco:Integer"/>
      ..
      <xsl:apply-templates select="$range/gco:upper"/>
			</div>
		</div>
	</xsl:template>
	<xsl:template match="gco:upper">
		<xsl:choose>
			<xsl:when test="gco:UnlimitedInteger/@isInfinite">
      *
    </xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="gco:UnlimitedInteger"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="gfc:FC_ListedValue">
		<li>
			<xsl:value-of select="gfc:label/gco:CharacterString"/>
		</li>
	</xsl:template>
	<xsl:template match="gfc:constrainedBy">
		<li>
			<xsl:value-of select="gfc:FC_Constraint/gfc:description/gco:CharacterString"/>
		</li>
	</xsl:template>
</xsl:stylesheet>

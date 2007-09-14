﻿<?xml version="1.0" encoding="utf-8"?>
<!--
	Copyright © Neumont University. All rights reserved.

	This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
	Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:
	1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
	2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
	3. This notice may not be removed or altered from any source distribution.
-->
<!-- Contributors: Corey Kaylor, Kevin M. Owen, Clé Diggins -->
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:exsl="http://exslt.org/common"
	xmlns:dml="http://schemas.orm.net/DIL/DMIL"
	xmlns:dms="http://schemas.orm.net/DIL/DILMS"
	xmlns:dep="http://schemas.orm.net/DIL/DILEP"
	xmlns:ddt="http://schemas.orm.net/DIL/DILDT"
	xmlns:dil="http://schemas.orm.net/DIL/DIL"
	xmlns:ddl="http://schemas.orm.net/DIL/DDIL"
	extension-element-prefixes="exsl"
	exclude-result-prefixes="dml dms dep ddt dil ddl">

	<xsl:import href="DDILtoSQLStandard.xslt"/>
	<xsl:import href="TruthValueTestRemover.xslt"/>
	<xsl:import href="DomainInliner.xslt"/>

	<xsl:output method="text" encoding="utf-8" indent="no" omit-xml-declaration="yes"/>
	<xsl:strip-space elements="*"/>

	<xsl:param name="StatementDelimeter">
		<xsl:value-of select="$NewLine"/>
		<xsl:text>GO</xsl:text>
		<xsl:value-of select="$NewLine"/>
	</xsl:param>
	<xsl:param name="StatementBlockDelimeter">
		<xsl:value-of select="$NewLine"/>
		<xsl:text>GO</xsl:text>
	</xsl:param>

	<xsl:template match="/">
		<xsl:variable name="truthValueTestRemovedDilFragment">
			<xsl:apply-templates mode="TruthValueTestRemover" select="."/>
		</xsl:variable>
		<xsl:variable name="domainInlinedDilFragment">
			<xsl:apply-templates mode="DomainInliner" select="exsl:node-set($truthValueTestRemovedDilFragment)/child::*"/>
		</xsl:variable>
		<xsl:apply-templates select="exsl:node-set($domainInlinedDilFragment)/child::*"/>
	</xsl:template>

	<xsl:template match="ddl:schemaDefinition">
		<xsl:param name="indent"/>
		<!--<xsl:text>DROP SCHEMA </xsl:text>
		<xsl:apply-templates select="@catalogName" mode="ForSchemaDefinition"/>
		<xsl:apply-templates select="@schemaName" mode="ForSchemaDefinition"/>
		<xsl:apply-templates select="@authorizationIdentifier" mode="ForSchemaDefinition"/>
		<xsl:apply-templates select="@defaultCharacterSet" mode="ForSchemaDefinition"/>
		<xsl:apply-templates select="ddl:path" mode="ForSchemaDefinition"/>
		<xsl:value-of select="$StatementDelimeter"/>
		<xsl:value-of select="$NewLine"/>
		<xsl:text>GO</xsl:text>
		<xsl:value-of select="$NewLine"/>
		<xsl:value-of select="$indent"/>-->
		<xsl:text>CREATE SCHEMA </xsl:text>
		<xsl:apply-templates select="@catalogName" mode="ForSchemaDefinition"/>
		<xsl:apply-templates select="@schemaName" mode="ForSchemaDefinition"/>
		<xsl:apply-templates select="@authorizationIdentifier" mode="ForSchemaDefinition"/>
		<xsl:apply-templates select="@defaultCharacterSet" mode="ForSchemaDefinition"/>
		<xsl:apply-templates select="ddl:path" mode="ForSchemaDefinition"/>
		<xsl:value-of select="$StatementDelimeter"/>
		<xsl:value-of select="$NewLine"/>
		<xsl:text>GO</xsl:text>
		<xsl:value-of select="$NewLine"/>
		<xsl:call-template name="writeOutStartTrans" />
		<xsl:value-of select="$NewLine"/>
		<xsl:apply-templates>
			<xsl:with-param name="indent" select="concat($indent, $IndentChar)"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="dms:startTransactionStatement">
	</xsl:template>
	
	<xsl:template match="dms:startTransactionStatement" mode="writeOut" name="writeOutStartTrans">
		<!-- Atomicity has been removed becuase multiple sprocs can't be in transaction in t-sql.-->
	</xsl:template>
	
	<xsl:template match="dms:commitStatement">
		<xsl:value-of select="$StatementBlockDelimeter"/>
	</xsl:template>

	<xsl:template match="@defaultCharacterSet" mode="ForSchemaDefinition"/>

	<xsl:template match="dms:setSchemaStatement"/>

	<xsl:template match="ddl:identityColumnSpecification">
		<xsl:text> IDENTITY </xsl:text>
		<xsl:value-of select="$LeftParen"/>
		<xsl:apply-templates select="child::*[1]"/>
		<xsl:text>, </xsl:text>
		<xsl:apply-templates select="child::*[2]"/>
		<xsl:value-of select="$RightParen"/>
	</xsl:template>

	<xsl:template match="ddl:sequenceGeneratorStartWithOption">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddl:sequenceGeneratorIncrementByOption">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="@type[.='BOOLEAN']" mode="ForDataType">
		<xsl:text>BIT</xsl:text>
	</xsl:template>

	<xsl:template match="@type[.='CHARACTER' or .='CHARACTER VARYING']" mode="ForDataType">
		<xsl:text>NATIONAL </xsl:text>
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template match="@type[.='DATE' or .='TIME']" mode="ForDataType">
		<xsl:text>DATETIME</xsl:text>
	</xsl:template>

	<xsl:template match="ddt:booleanLiteral">
		<xsl:choose>
			<xsl:when test="@value='TRUE'">
				<xsl:text>1</xsl:text>
			</xsl:when>
			<xsl:when test="@value='FALSE'">
				<xsl:text>0</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>NULL</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="dep:charLengthExpression">
		<xsl:text>LEN</xsl:text>
		<xsl:value-of select="$LeftParen"/>
		<xsl:apply-templates/>
		<xsl:value-of select="$RightParen"/>
	</xsl:template>

	<xsl:template match="@lengthUnits"/>

	<xsl:template match="ddl:sqlParameterDeclaration">
		<xsl:value-of select="$IndentChar" />
		<xsl:value-of select="'@'"/>
		<xsl:value-of select="@name"/>
		<xsl:text> </xsl:text>
		<xsl:apply-templates select="child::*" />
		<xsl:if test="not(position()=last())">
			<xsl:text>, </xsl:text>
		</xsl:if>
		<xsl:value-of select="$NewLine"/>
	</xsl:template>

	<xsl:template match="dep:sqlParameterReference">
		<xsl:value-of select="'@'"/>
		<xsl:value-of select="@name"/>
		<xsl:if test="not(position()=last())">
			<xsl:text>, </xsl:text>
		</xsl:if>
	</xsl:template>

	<xsl:template match="dep:trimFunction">
		<xsl:choose>
			<xsl:when test="@specification='BOTH'">
				<xsl:text>LTRIM</xsl:text>
				<xsl:value-of select="$LeftParen"/>
				<xsl:text>RTRIM</xsl:text>
				<xsl:value-of select="$LeftParen"/>
				<xsl:apply-templates select="dep:trimSource"/>
				<xsl:value-of select="$RightParen"/>
				<xsl:value-of select="$RightParen"/>
			</xsl:when>
			<xsl:when test="@specification='LEADING'">
				<xsl:text>LTRIM</xsl:text>
				<xsl:value-of select="$LeftParen"/>
				<xsl:apply-templates select="dep:trimSource"/>
				<xsl:value-of select="$RightParen"/>
			</xsl:when>
			<xsl:when test="@specification='TRAILING'">
				<xsl:text>RTRIM</xsl:text>
				<xsl:value-of select="$LeftParen"/>
				<xsl:apply-templates select="dep:trimSource"/>
				<xsl:value-of select="$RightParen"/>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@onUpdate" mode="ForReferenceSpecification">
		<xsl:text> ON UPDATE </xsl:text>
		<xsl:choose>
			<xsl:when test=".='RESTRICT'">
				<xsl:text>NO ACTION</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="."/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@onDelete" mode="ForReferenceSpecification">
		<xsl:text> ON DELETE </xsl:text>
		<xsl:choose>
			<xsl:when test=".='RESTRICT'">
				<xsl:text>NO ACTION</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="."/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!--<xsl:template match="ddl:sqlInvokedProcedure">
		<xsl:value-of select="$NewLine"/>
		<xsl:text>CREATE PROCEDURE </xsl:text>
		<xsl:if test="@schema">
			<xsl:value-of select="@schema"/>
			<xsl:text>.</xsl:text>
		</xsl:if>
		<xsl:value-of select="@name"/>
		<xsl:value-of select="$NewLine"/>
		<xsl:value-of select="$LeftParen"/>
		<xsl:value-of select="$NewLine"/>
		<xsl:apply-templates select="ddl:sqlParameterDeclaration" />
		<xsl:value-of select="$RightParen"/>
		<xsl:value-of select="$NewLine"/>
		<xsl:apply-templates select="ddl:sqlRoutineSpec" />
		<xsl:value-of select="$StatementDelimeter"/>
		<xsl:value-of select="$NewLine"/>
	</xsl:template>-->

	<xsl:template match="dep:constraintNameDefinition">
		<xsl:param name="tableName"/>
		<xsl:text>CONSTRAINT </xsl:text>
		<xsl:if test="@schema">
			<xsl:value-of select="@schema"/>
			<xsl:text>.</xsl:text>
		</xsl:if>
		<xsl:choose>
			<xsl:when test="$tableName">
				<xsl:choose>
					<xsl:when test="contains($tableName, '&quot;')">
						<xsl:variable name="tablePart1" select="substring-after($tableName, '&quot;')"/>
						<xsl:variable name="table" select="substring-before($tablePart1, '&quot;')"/>
						<xsl:text>&quot;</xsl:text>
						<xsl:value-of select="concat($table, @name)"/>
						<xsl:text>&quot;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:variable name="table" select="concat($tableName, '_')"/>
						<xsl:value-of select="concat($table, @name)"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@name"/>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text> </xsl:text>
		<xsl:apply-templates/>
	</xsl:template>

</xsl:stylesheet>

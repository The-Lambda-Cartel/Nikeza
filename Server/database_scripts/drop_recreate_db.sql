USE [master]
GO

/****** Object:  Database [Nikeza]    Script Date: 8/5/2017 7:56:58 AM ******/
DROP DATABASE [Nikeza]
GO

/****** Object:  Database [Nikeza]    Script Date: 8/5/2017 7:56:58 AM ******/
CREATE DATABASE [Nikeza]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Nikeza', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\Nikeza.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Nikeza_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\Nikeza_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO

ALTER DATABASE [Nikeza] SET COMPATIBILITY_LEVEL = 130
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Nikeza].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [Nikeza] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [Nikeza] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [Nikeza] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [Nikeza] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [Nikeza] SET ARITHABORT OFF 
GO

ALTER DATABASE [Nikeza] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [Nikeza] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [Nikeza] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [Nikeza] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [Nikeza] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [Nikeza] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [Nikeza] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [Nikeza] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [Nikeza] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [Nikeza] SET  DISABLE_BROKER 
GO

ALTER DATABASE [Nikeza] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [Nikeza] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [Nikeza] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [Nikeza] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [Nikeza] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [Nikeza] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [Nikeza] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [Nikeza] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [Nikeza] SET  MULTI_USER 
GO

ALTER DATABASE [Nikeza] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [Nikeza] SET DB_CHAINING OFF 
GO

ALTER DATABASE [Nikeza] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [Nikeza] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [Nikeza] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [Nikeza] SET QUERY_STORE = OFF
GO

USE [Nikeza]
GO

ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO

ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO

ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO

ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO

ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO

ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO

ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO

ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO

ALTER DATABASE [Nikeza] SET  READ_WRITE 
GO


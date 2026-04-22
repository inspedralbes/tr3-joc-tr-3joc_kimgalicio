-- database.sql
-- Script de creació de la base de dades per al joc TickTack-Tag.
-- Conté les taules necessàries per emmagatzemar usuaris i partides.
--
-- Execució: mysql -u root -p < database.sql
-- O des de MySQL Workbench: File > Run SQL Script > seleccionar aquest fitxer.

-- ─────────────────────────────────────────────────────────────────────────────
-- 1. CREACIÓ DE LA BASE DE DADES
-- ─────────────────────────────────────────────────────────────────────────────

-- Creem la base de dades si no existeix i l'activem.
-- utf8mb4 suporta emojis i caràcters especials.
CREATE DATABASE IF NOT EXISTS ticktack_tag
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE ticktack_tag;

-- ─────────────────────────────────────────────────────────────────────────────
-- 2. TAULA: Users
-- Emmagatzema els jugadors registrats al joc.
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS Users (
  -- Identificador únic auto-incrementat de l'usuari.
  id       INT          NOT NULL AUTO_INCREMENT,

  -- Nom de jugador únic (màxim 50 caràcters).
  nickname VARCHAR(50)  NOT NULL UNIQUE,

  -- Nombre total de partides guanyades. Per defecte, 0.
  wins     INT          NOT NULL DEFAULT 0,

  -- Nombre total de partides perdudes. Per defecte, 0.
  losses   INT          NOT NULL DEFAULT 0,

  PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ─────────────────────────────────────────────────────────────────────────────
-- 3. TAULA: Games
-- Emmagatzema l'historial de partides jugades.
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS Games (
  -- Identificador únic auto-incrementat de la partida.
  id          INT          NOT NULL AUTO_INCREMENT,

  -- Modalitat de joc: 'vs_bot' (contra la IA) o 'vs_player' (multijugador).
  mode        VARCHAR(20)  NOT NULL,

  -- Jugador que ha creat la partida (clau forana a Users).
  player1_id  INT          NOT NULL,

  -- Segon jugador (null si és 'vs_bot' o si la partida és 'pending').
  player2_id  INT          NULL,

  -- Estat de la partida: 'pending', 'playing' o 'finished'.
  status      VARCHAR(20)  NOT NULL DEFAULT 'pending',

  -- Guanyador de la partida (null fins que la partida acabi).
  winner_id   INT          NULL,

  -- Vides restants del guanyador al finalitzar la partida.
  winner_hearts INT        NOT NULL DEFAULT 0,

  -- Restriccions de clau forana per garantir la integritat referencial.
  PRIMARY KEY (id),
  CONSTRAINT fk_games_player1 FOREIGN KEY (player1_id) REFERENCES Users(id),
  CONSTRAINT fk_games_player2 FOREIGN KEY (player2_id) REFERENCES Users(id),
  CONSTRAINT fk_games_winner  FOREIGN KEY (winner_id)  REFERENCES Users(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

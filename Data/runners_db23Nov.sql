-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 23-11-2025 a las 12:13:04
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `runners_db`
--
CREATE DATABASE IF NOT EXISTS `runners_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `runners_db`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `categorias_evento`
--

CREATE TABLE `categorias_evento` (
  `idCategoria` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL COMMENT 'Ej: 10k Competitiva, 2k Corre Caminata',
  `costoInscripcion` decimal(10,2) NOT NULL DEFAULT 0.00,
  `cupoCategoria` int(11) DEFAULT NULL,
  `edadMinima` int(11) DEFAULT 0,
  `edadMaxima` int(11) DEFAULT 99,
  `genero` enum('F','M','X') DEFAULT 'X' COMMENT 'Categoría aplica a Femenino, Masculino o Mixto/Todos (X)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `eventos`
--

CREATE TABLE `eventos` (
  `idEvento` int(11) NOT NULL,
  `nombre` varchar(255) NOT NULL,
  `descripcion` text DEFAULT NULL,
  `fechaHora` datetime NOT NULL,
  `lugar` varchar(255) NOT NULL,
  `cupoTotal` int(11) DEFAULT NULL,
  `idOrganizador` int(11) NOT NULL,
  `urlPronosticoClima` varchar(255) DEFAULT NULL,
  `datosPago` text DEFAULT NULL COMMENT 'Datos para transferencia (CBU, Alias, Titular), inicialmente puede ser nulo hasta que el orga cargue datos de alias',
  `estado` enum('publicado','cancelado','finalizado') NOT NULL DEFAULT 'publicado'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inscripciones`
--

CREATE TABLE `inscripciones` (
  `idInscripcion` int(11) NOT NULL,
  `idUsuario` int(11) NOT NULL,
  `idCategoria` int(11) NOT NULL,
  `fechaInscripcion` datetime DEFAULT current_timestamp(),
  `estadoPago` enum('pendiente','en_revision','pagado','rechazado') DEFAULT 'pendiente',
  `talleRemera` enum('XS','S','M','L','XL','XXL') DEFAULT NULL,
  `aceptoDeslinde` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Debe ser true (1) para aceptar el deslinde',
  `comprobantePagoURL` varchar(255) DEFAULT NULL COMMENT 'URL o path al comprobante subido'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `notificaciones_evento`
--

CREATE TABLE `notificaciones_evento` (
  `idNotificacion` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL COMMENT 'Evento al que se asocia',
  `titulo` varchar(255) NOT NULL COMMENT 'Ej: Evento Suspendido',
  `mensaje` text DEFAULT NULL COMMENT 'Ej: La carrera se pasa al próximo domingo...',
  `fechaEnvio` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `perfiles_organizadores`
--

CREATE TABLE `perfiles_organizadores` (
  `idPerfilOrganizador` int(11) NOT NULL,
  `idUsuario` int(11) NOT NULL COMMENT 'FK a la tabla usuarios',
  `razonSocial` varchar(100) NOT NULL COMMENT 'Nombre Legal y Oficial de la entidad',
  `nombreComercial` varchar(100) NOT NULL COMMENT 'Nombre de marca que se usa públicamente (puede ser igual al campo nombre en usuarios)',
  `cuit_taxid` varchar(30) NOT NULL COMMENT 'CUIT/ID Fiscal de la organización',
  `direccionLegal` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `perfiles_runners`
--

CREATE TABLE `perfiles_runners` (
  `idPerfilRunner` int(11) NOT NULL,
  `idUsuario` int(11) NOT NULL COMMENT 'FK a la tabla usuarios',
  `nombre` varchar(100) NOT NULL COMMENT 'Nombre de Pila del Runner',
  `apellido` varchar(100) NOT NULL COMMENT 'Apellido del Runner',
  `fechaNacimiento` datetime NOT NULL,
  `genero` enum('F','M','X') NOT NULL,
  `dni` int(11) NOT NULL COMMENT 'DNI del Runner',
  `localidad` varchar(100) NOT NULL,
  `agrupacion` varchar(100) NOT NULL COMMENT 'Agrupacin o libre (si no tiene)',
  `telefonoEmergencia` varchar(50) NOT NULL COMMENT 'Contacto de emergencia'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `perfiles_runners`
--

INSERT INTO `perfiles_runners` (`idPerfilRunner`, `idUsuario`, `nombre`, `apellido`, `fechaNacimiento`, `genero`, `dni`, `localidad`, `agrupacion`, `telefonoEmergencia`) VALUES
(2, 2, 'Carlos', 'Gonzalez', '1990-03-20 00:00:00', 'M', 11111111, 'San Luis', 'Libre', '2664111112');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `puntosinteres`
--

CREATE TABLE `puntosinteres` (
  `idPuntoInteres` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL,
  `tipo` enum('hidratacion','primeros_auxilios','meta','largada','otro') NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `latitud` decimal(10,7) NOT NULL,
  `longitud` decimal(10,7) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `resultados`
--

CREATE TABLE `resultados` (
  `idResultado` int(11) NOT NULL,
  `idInscripcion` int(11) NOT NULL,
  `tiempoOficial` varchar(20) DEFAULT NULL COMMENT 'Ej: 00:45:30.123',
  `posicionGeneral` int(11) DEFAULT NULL,
  `posicionCategoria` int(11) DEFAULT NULL,
  `tiempoSmartwatch` varchar(20) DEFAULT NULL COMMENT 'Ej: 00:45:28',
  `distanciaKm` decimal(6,2) DEFAULT NULL COMMENT 'Ej: 10.02',
  `ritmoPromedio` varchar(20) DEFAULT NULL COMMENT 'Ej: 4:32 min/km',
  `velocidadPromedio` varchar(20) DEFAULT NULL COMMENT 'Ej: 13.2 km/h',
  `caloriasQuemadas` int(11) DEFAULT NULL,
  `pulsacionesPromedio` int(11) DEFAULT NULL,
  `pulsacionesMax` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `rutas`
--

CREATE TABLE `rutas` (
  `idRuta` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL,
  `orden` int(11) NOT NULL COMMENT 'Orden del punto en el trazado',
  `latitud` decimal(10,7) NOT NULL,
  `longitud` decimal(10,7) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `idUsuario` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL COMMENT 'Nombre de Pila (Runner) o Nombre Comercial (Organizador)',
  `email` varchar(100) NOT NULL,
  `telefono` varchar(20) NOT NULL COMMENT 'Numero de celu',
  `passwordHash` varchar(255) NOT NULL,
  `tipoUsuario` enum('runner','organizador') NOT NULL,
  `estado` tinyint(1) NOT NULL DEFAULT 1 COMMENT '0 false, 1 true (Al crear) sera de estado true',
  `imgAvatar` varchar(500) DEFAULT NULL COMMENT 'URL o ruta del avatar del usuario'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`idUsuario`, `nombre`, `email`, `telefono`, `passwordHash`, `tipoUsuario`, `estado`, `imgAvatar`) VALUES
(2, 'Carlos', 'carlos@test.com', '2664111111', '$2a$12$1eEF/knMNucRBPUTHbWD1Ortnqnafy4rIlyGPrDSAKe10NIAKaRri', 'runner', 1, '/uploads/avatars/defaults/default_runner.png');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `categorias_evento`
--
ALTER TABLE `categorias_evento`
  ADD PRIMARY KEY (`idCategoria`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indices de la tabla `eventos`
--
ALTER TABLE `eventos`
  ADD PRIMARY KEY (`idEvento`),
  ADD KEY `idOrganizador` (`idOrganizador`);

--
-- Indices de la tabla `inscripciones`
--
ALTER TABLE `inscripciones`
  ADD PRIMARY KEY (`idInscripcion`),
  ADD KEY `idUsuario` (`idUsuario`),
  ADD KEY `idCategoria` (`idCategoria`);

--
-- Indices de la tabla `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  ADD PRIMARY KEY (`idNotificacion`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indices de la tabla `perfiles_organizadores`
--
ALTER TABLE `perfiles_organizadores`
  ADD PRIMARY KEY (`idPerfilOrganizador`),
  ADD UNIQUE KEY `razonSocial` (`razonSocial`),
  ADD UNIQUE KEY `cuit_taxid` (`cuit_taxid`),
  ADD UNIQUE KEY `idUsuario_UNIQUE` (`idUsuario`);

--
-- Indices de la tabla `perfiles_runners`
--
ALTER TABLE `perfiles_runners`
  ADD PRIMARY KEY (`idPerfilRunner`),
  ADD UNIQUE KEY `dni` (`dni`),
  ADD UNIQUE KEY `idUsuario_UNIQUE` (`idUsuario`);

--
-- Indices de la tabla `puntosinteres`
--
ALTER TABLE `puntosinteres`
  ADD PRIMARY KEY (`idPuntoInteres`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indices de la tabla `resultados`
--
ALTER TABLE `resultados`
  ADD PRIMARY KEY (`idResultado`),
  ADD UNIQUE KEY `idInscripcion` (`idInscripcion`) COMMENT 'Garantiza Relación 1:1';

--
-- Indices de la tabla `rutas`
--
ALTER TABLE `rutas`
  ADD PRIMARY KEY (`idRuta`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`idUsuario`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `email_2` (`email`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `categorias_evento`
--
ALTER TABLE `categorias_evento`
  MODIFY `idCategoria` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `eventos`
--
ALTER TABLE `eventos`
  MODIFY `idEvento` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `inscripciones`
--
ALTER TABLE `inscripciones`
  MODIFY `idInscripcion` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  MODIFY `idNotificacion` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `perfiles_organizadores`
--
ALTER TABLE `perfiles_organizadores`
  MODIFY `idPerfilOrganizador` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `perfiles_runners`
--
ALTER TABLE `perfiles_runners`
  MODIFY `idPerfilRunner` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT de la tabla `puntosinteres`
--
ALTER TABLE `puntosinteres`
  MODIFY `idPuntoInteres` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `resultados`
--
ALTER TABLE `resultados`
  MODIFY `idResultado` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `rutas`
--
ALTER TABLE `rutas`
  MODIFY `idRuta` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `idUsuario` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `categorias_evento`
--
ALTER TABLE `categorias_evento`
  ADD CONSTRAINT `categorias_evento_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Filtros para la tabla `eventos`
--
ALTER TABLE `eventos`
  ADD CONSTRAINT `eventos_ibfk_1` FOREIGN KEY (`idOrganizador`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE;

--
-- Filtros para la tabla `inscripciones`
--
ALTER TABLE `inscripciones`
  ADD CONSTRAINT `inscripciones_ibfk_1` FOREIGN KEY (`idUsuario`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE,
  ADD CONSTRAINT `inscripciones_ibfk_2` FOREIGN KEY (`idCategoria`) REFERENCES `categorias_evento` (`idCategoria`) ON DELETE CASCADE;

--
-- Filtros para la tabla `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  ADD CONSTRAINT `notificaciones_evento_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Filtros para la tabla `perfiles_organizadores`
--
ALTER TABLE `perfiles_organizadores`
  ADD CONSTRAINT `fk_perfiles_organizadores_usuarios` FOREIGN KEY (`idUsuario`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE;

--
-- Filtros para la tabla `perfiles_runners`
--
ALTER TABLE `perfiles_runners`
  ADD CONSTRAINT `fk_perfiles_runners_usuarios` FOREIGN KEY (`idUsuario`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE;

--
-- Filtros para la tabla `puntosinteres`
--
ALTER TABLE `puntosinteres`
  ADD CONSTRAINT `puntosinteres_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Filtros para la tabla `resultados`
--
ALTER TABLE `resultados`
  ADD CONSTRAINT `resultados_ibfk_1` FOREIGN KEY (`idInscripcion`) REFERENCES `inscripciones` (`idInscripcion`) ON DELETE CASCADE;

--
-- Filtros para la tabla `rutas`
--
ALTER TABLE `rutas`
  ADD CONSTRAINT `rutas_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

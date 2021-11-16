--
-- PostgreSQL database dump
--

-- Dumped from database version 11.5
-- Dumped by pg_dump version 11.5

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: station; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.station (
    station_id character varying,
    temp integer,
    windspeed integer,
    neighborhood character varying
);


ALTER TABLE public.station OWNER TO postgres;

--
-- Data for Name: station; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.station (station_id, temp, windspeed, neighborhood) FROM stdin;
IVIENNA316	\N	\N	\N
IDONAU116	16	5	Donaustadt
IDONAU59	14	5	Donaustadt
IDONAU61	15	21	Donaustadt
IDONAU77	14	13	Donaustadt
IDONAU88	15	6	Donaustadt
IDONAU48	15	2	Donaustadt
IDONAU9	15	5	Donaustadt
IDONAU86	15	6	Donaustadt
IDONAU63	15	3	Donaustadt
IDONAU36	15	7	Donaustadt
IDONAU113	14	2	Donaustadt
IDONAU87	14	2	Donaustadt
IDONAU100	15	9	Donaustadt
IDONAU1	15	7	Donaustadt
IDONAU99	16	13	Donaustadt
IDONAU102	16	1	Donaustadt
IFLORI81	14	9	Floridsdorf
IFLORI81	14	9	Floridsdorf
IDONAU15	15	14	Donaustadt
IFLORI62	14	1	Floridsdorf
IFLORI62	14	1	Floridsdorf
IDONAU69	15	8	Donaustadt
IDONAU89	14	15	Donaustadt
IDONAU55	\N	\N	\N
ILIESI8	\N	\N	\N
ILIESI17	\N	\N	\N
ILIESI29	\N	\N	\N
ILIESI10	\N	\N	\N
IBRIGI5	\N	\N	\N
IDBLIN5	\N	\N	\N
IWHRIN2	\N	\N	\N
IWHRIN7	\N	\N	\N
IPENZI8	\N	\N	\N
IPENZI21	\N	\N	\N
IHIETZIN10	\N	\N	\N
IHIETZ5	\N	\N	\N
IMEIDL6	\N	\N	\N
ISIMME6	\N	\N	\N
ISIMMERI3	\N	\N	\N
IFAVOR31	\N	\N	\N
IFAVOR3	\N	\N	\N
IFAVOR4	\N	\N	\N
IDONAU79	15	9	Donaustadt
IDONAU50	15	4	Donaustadt
IDONAU96	15	7	Donaustadt
IDONAU117	13	17	Donaustadt
IFLORI65	15	4	Floridsdorf
IDONAU45	15	1	Donaustadt
IDONAU114	15	16	Donaustadt
ILIESI2	14	11	Liesing
ILIESI18	15	5	Liesing
ILIESI19	14	14	Liesing
ILIESI30	15	0	Liesing
ILIESI11	17	9	Liesing
ILIESI3	17	4	Liesing
ILIESI32	15	9	Liesing
ILIESI24	14	10	Liesing
ILIESI21	14	12	Liesing
ILIESI5	15	10	Liesing
IBRIGI11	14	3	Brigittenau
IBRIGI14	15	3	Brigittenau
IDBLIN4	16	5	Döbling
IBRIGI9	13	15	Brigittenau
IDBLIN1	14	13	Döbling
IDBLIN6	15	6	Döbling
IDBLIN3	14	8	Döbling
IDBLIN2	14	14	Döbling
IWHRIN9	15	5	Währing
IWHRIN5	14	5	Währing
IHERNA18	14	8	Hernals
IHERNA12	14	2	Hernals
IOTTAK10	15	3	Ottakring
IHERNA14	15	4	Hernals
IOTTAK12	16	9	Ottakring
IOTTAK13	14	4	Ottakring
IOTTAK3	15	1	Ottakring
IRUDOL8	14	9	Rudolfsheim-Fünfhaus
IRUDOLFS16	15	8	City
IPENZI6	14	8	Penzing
IPENZI43	14	3	Penzing
IPENZI10	17	0	Penzing
IPENZI42	14	0	Penzing
IPENZI27	14	5	Penzing
IPENZI33	15	5	Penzing
IPENZI31	14	10	Penzing
IPENZI50	18	2	Penzing
IPENZI47	14	13	Penzing
IPENZI14	15	11	Penzing
IPENZI39	15	11	Penzing
IHIETZIN11	15	6	KGV Wattmanngasse
IHIETZIN9	14	1	au.home
IHIETZIN5	14	2	Vienna
IHIETZIN15	15	6	sonni
IHIETZ11	15	6	Hietzing
IHIETZ10	14	6	Hietzing
IHIETZ1	15	3	Hietzing
IMEIDL15	14	8	Meidling
IMEIDL16	15	12	Meidling
IMEIDL9	14	0	Meidling
IMEIDL17	15	4	Meidling
IMEIDLIN5	14	24	Vienna Schloeglgasse
IMEIDLIN4	14	18	Hetzendorf
ISIMME21	15	2	Simmering
ISIMME18	15	2	Simmering
ISIMME103	14	5	Simmering
ISIMME5	16	5	Simmering
ISIMME102	14	2	Simmering
ISIMME113	16	3	Simmering
ISIMMERI7	15	8	My Home
IFAVOR6	15	3	Favoriten
IFAVOR1	15	16	Favoriten
IFAVOR34	14	1	Favoriten
IFLORIDS6	15	3	Stammersdorf
IFAVOR23	15	8	Favoriten
IFAVOR2	14	24	Favoriten
IFAVOR5	15	4	Favoriten
IFLORI1	15	10	Floridsdorf
IFLORI98	15	15	Floridsdorf
IFLORI95	15	9	Floridsdorf
IFLORIDS5	16	16	Leopoldau
IFLORI12	16	12	Floridsdorf
IFLORI5	15	6	Floridsdorf
IWIENWIE47	15	21	Wien Floridsdorf
IFLORI104	14	25	Floridsdorf
IFLORI61	14	3	Floridsdorf
IFLORI113	14	13	Floridsdorf
IFLORI109	14	14	Floridsdorf
IFLORIDS3	15	6	Orasteig
IDONAU98	15	3	Donaustadt
IDONAU83	15	1	Donaustadt
IFAVORIT9	\N	\N	\N
IALSER6	\N	\N	\N
INEUBA8	\N	\N	\N
IMARIA24	\N	\N	\N
IMARIA92	\N	\N	\N
IMARIAHI2	\N	\N	\N
IWIEDE4	\N	\N	\N
ILANDS75	\N	\N	\N
ILANDSTR3	\N	\N	\N
ILEOPO50	\N	\N	\N
IMARIA22	12	7	Mariapfarr
IMARIA81	13	6	Maria Laach am Jauerling
IMARIA118	13	0	Maria Laach am Jauerling
IMARIA18	15	9	Mariahilf
IMARGA1	15	18	Margareten
IMARIA40	12	2	Mariapfarr
IMARIA54	22	14	Maria Neustift
IWIEDE3	16	8	Wieden
IWIEDE9	14	16	Wieden
IWIEDE10	15	15	Wieden
IWIEDE13	15	15	Wieden
IWIEDEN4	15	13	Kleinpertenschlag
IWIEDEN14	6	0	easyweater
IWIEDEN16	11	0	Alpengasthof Bidner Wetter
ILANDS73	15	1	Landstraße
ILEOPO52	15	5	Leopoldstadt
ILEOPO75	13	1	Leopoldschlag
ILEOPO53	14	11	Leopoldsdorf
ILEOPO51	16	0	Leopoldschlag
ILEOPO71	15	7	Leopoldsdorf im Marchfelde
ILEOPO60	15	2	Leopoldstadt
ILEOPO77	16	9	Leopoldstadt
ILEOPO69	13	4	Leopoldschlag
ILEOPO23	15	1	Leopoldsdorf
ILEOPO74	14	5	Leopoldschlag
ILEOPO31	15	7	Leopoldstadt
ILEOPOLD57	14	12	Breitstetten
ILEOPOLD59	14	16	Breitstetten
ILEOPOLD64	15	9	KGV Unterer Prater
IINNER5	13	0	Innerbraz
IINNER9	14	2	Innere Stadt
IINNER11	15	0	Innere Stadt
IVIENNA242	15	2	Aspern
IMARIA4	14	9	Maria Laach am Jauerling
IVIENNA341	14	18	Ville Verdi
IHERNA15	13	16	Hernals
IHERNA5	16	5	Hernals
IOTTAK18	14	15	Ottakring
IOTTAK9	15	8	Ottakring
IRUDOL1	15	3	Rudolfsheim-Fünfhaus
IRUDOL2	-40	6	Rudolfsheim-Fünfhaus
IPENZI11	15	2	Penzing
IPENZI5	15	6	Penzing
IVIENNA262	\N	\N	\N
IVIENNA337	\N	\N	\N
IVIENNA315	\N	\N	\N
IWIENWIE27	\N	\N	\N
IWIENWIE24	\N	\N	\N
IPENZI34	14	4	Penzing
IPENZI40	13	18	Penzing
IPENZI4	14	0	Penzing
IPENZI49	15	2	Penzing
IPENZI16	15	18	Penzing
IHIETZIN4	14	15	Lainzer Tiergarten
IHIETZIN12	15	4	Paradies
IHIETZ7	15	3	Hietzing
IHIETZ9	15	3	Hietzing
IMEIDL7	15	9	Meidling
IMEIDL10	15	15	Meidling
IMEIDLIN2	15	16	Sagedergasse 21A
IMEIDLIN7	15	4	Gartenfreunde XII
ISIMME16	15	6	Simmering
ISIMME12	15	6	Simmering
ISIMME101	16	5	Simmering
IFAVOR14	17	5	Favoriten
IFAVOR33	15	14	Favoriten
IFAVOR22	14	13	Favoriten
IFAVOR12	15	3	Favoriten
IFAVORIT3	14	21	Reumannplatz
IFAVORIT5	15	32	LAAERBERG
IFAVORIT4	14	7	Favoriten, nähe Raxstraße
IFAVORIT6	15	10	Raxstr. / Laxenburgerstr.
IALSER5	15	10	Alsergrund
IALSER4	15	3	Alsergrund
IALSER7	15	2	Alsergrund
IJOSEFST2	15	0	Town
IJOSEFST3	15	3	Hamerlingplatz Dachterasse
INEUBA1	16	8	Neubau
INEUBA7	15	8	Neubau
INEUBAU4	15	3	Wien Neubau
IMARIA91	17	1	Mariazell
IMARIA110	14	14	Maria Enzersdorf
ILEOPO28	13	1	Leopoldschlag
IWIENWIE4	14	11	1030 Wien
IVIENNA333	14	4	Kagran, Freihofsiedlung
IVIENNA338	14	5	Wien, Brigittenau
IVIENNA317	16	14	Ober St. Veit
IWIENWIE59	16	0	Ottakring
IWIENWIE25	15	5	1220 Wien Kagran
IVIENNA342	14	5	Satzingerweg
IWHRIN10	15	1	Währing
IWIENWIE87	15	32	Rothneusiedl
IWIENWIE52	15	24	Pretschgasse
IWIENWIE64	14	9	Stadlbreitener Anger
IWIENWIE80	\N	\N	\N
IWIENWIE37	\N	\N	\N
IWIENWIE61	\N	\N	\N
IWIENWIE54	\N	\N	\N
IWIENWIE7	14	11	1140 Wien Huetteldorf / Allianz-Stadion
IWIENWIE75	15	9	Mühlwasser
IWIENWIE21	14	8	1160 Wien Ottakring / Kuffner Sternwarte
IWIENWIE71	14	4	1130 Wien, Roter Berg
IWIENWIE84	17	12	Wien, Simmering, Lindenbauergasse
IMARIA20	21	4	Maria Neustift
IMARIA104	20	7	Maria Neustift
IMARIA129	15	8	Maria Lanzendorf
IMARIA71	17	2	Mariastein
IMARIA21	14	10	Maria Laach am Jauerling
IMARIA11	16	4	Mariahilf
IMARIA33	22	12	Maria Neustift
IMARIA64	11	0	Maria Schmolln
IMARIA44	15	3	Maria Enzersdorf
IMARIA75	12	5	Maria Laach am Jauerling
IWIENWIE15	15	2	Austria, Vienna, Aspern, Lobau
IWIENWIE13	14	5	Schafberg
\.


--
-- PostgreSQL database dump complete
--


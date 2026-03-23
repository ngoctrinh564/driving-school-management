-- ============================================================
-- ====================        AUTH        ====================
-- ============================================================
CREATE OR REPLACE PROCEDURE PROC_LOGIN
(
    p_username IN NVARCHAR2,
    o_userId OUT NUMBER,
    o_roleId OUT NUMBER,
    o_username OUT NVARCHAR2,
    o_password OUT NVARCHAR2,
    o_isActive OUT NUMBER
)
AS
BEGIN
    SELECT userId, roleId, userName, "password", isActive
    INTO o_userId, o_roleId, o_username, o_password, o_isActive
    FROM "User"
    WHERE userName = p_username;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        o_userId := NULL;
END;
/

CREATE OR REPLACE PROCEDURE PROC_REGISTER
(
    p_username IN NVARCHAR2,
    p_password IN NVARCHAR2,
    p_roleId IN NUMBER,
    o_result OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM "User"
    WHERE userName = p_username;

    IF v_count > 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    INSERT INTO "User"(roleId, userName, "password", isActive)
    VALUES (p_roleId, p_username, p_password, 1);

    o_result := 1;
END;
/

CREATE OR REPLACE PROCEDURE PROC_RESET_PASSWORD
(
    p_username IN NVARCHAR2,
    p_newPassword IN NVARCHAR2,
    o_result OUT NUMBER
)
AS
BEGIN
    UPDATE "User"
    SET "password" = p_newPassword
    WHERE userName = p_username;

    IF SQL%ROWCOUNT = 0 THEN
        o_result := -1;
    ELSE
        o_result := 1;
    END IF;
END;
/

-- kiểm tra đã có những thông tin cần thiết trước khi đăng nhập vào
CREATE OR REPLACE FUNCTION FUNC_CHECK_USER_PROFILE
(
    p_userId IN NUMBER
)
RETURN NUMBER
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM HOCVIEN
    WHERE USERID = p_userId
      AND EMAIL IS NOT NULL
      AND SDT IS NOT NULL
      AND SOCMNDCCCD IS NOT NULL;

    IF v_count = 0 THEN
        RETURN 0; -- chưa đủ
    END IF;

    RETURN 1; -- đầy đủ
END;
/
-- ============================================================
-- ====================        ADMIN        ===================
-- ============================================================
-- DASHBOARD
CREATE OR REPLACE PROCEDURE PROC_ADMIN_DASHBOARD
(
    o_totalUser OUT NUMBER,
    o_totalHoSo OUT NUMBER,
    o_totalGPLX OUT NUMBER,
    o_totalBaiThi OUT NUMBER,
    o_userActive OUT NUMBER,
    o_userInactive OUT NUMBER,
    o_recentUsers OUT SYS_REFCURSOR
)
AS
BEGIN
    -- tổng user
    SELECT COUNT(*) INTO o_totalUser FROM "User";
    -- tổng hồ sơ
    SELECT COUNT(*) INTO o_totalHoSo FROM HoSoThiSinh;
    -- tổng GPLX
    SELECT COUNT(*) INTO o_totalGPLX FROM GiayPhepLaiXe;
    -- tổng bài thi
    SELECT COUNT(*) INTO o_totalBaiThi FROM BaiThi;
    -- user active
    SELECT COUNT(*) INTO o_userActive FROM "User" WHERE isActive = 1;
    -- user inactive
    SELECT COUNT(*) INTO o_userInactive FROM "User" WHERE isActive = 0;
    -- 5 user mới nhất
    OPEN o_recentUsers FOR
        SELECT *
        FROM (
            SELECT userId, userName, isActive
            FROM "User"
            ORDER BY userId DESC
        )
        WHERE ROWNUM <= 5;

END;
/
-- Exam: kì thi
CREATE OR REPLACE VIEW VW_KYTHI_ADMIN AS
SELECT 
    kt.kyThiId,
    kt.tenKyThi,
    kt.loaiKyThi,
    COUNT(ct.hoSoId) AS soLuongDangKy
FROM KyThi kt
LEFT JOIN ChiTietDangKyThi ct 
    ON kt.kyThiId = ct.kyThiId
GROUP BY 
    kt.kyThiId, 
    kt.tenKyThi, 
    kt.loaiKyThi;
    
/
CREATE OR REPLACE PROCEDURE SP_CREATE_KYTHI
(
    p_tenKyThi   KyThi.tenKyThi%TYPE,
    p_loaiKyThi  KyThi.loaiKyThi%TYPE
)
AS
BEGIN
    INSERT INTO KyThi (tenKyThi, loaiKyThi)
    VALUES (p_tenKyThi, p_loaiKyThi);
END;
/

CREATE OR REPLACE PROCEDURE SP_UPDATE_KYTHI
(
    p_kyThiId NUMBER,
    p_tenKyThi NVARCHAR2,
    p_loaiKyThi NVARCHAR2
)
AS
BEGIN
    UPDATE KyThi
    SET 
        tenKyThi = p_tenKyThi,
        loaiKyThi = p_loaiKyThi
    WHERE kyThiId = p_kyThiId;
END;
/
CREATE OR REPLACE PROCEDURE SP_DELETE_KYTHI
(
    p_kyThiId NUMBER
)
AS
BEGIN
    DELETE FROM KyThi
    WHERE kyThiId = p_kyThiId;
END;
/
CREATE OR REPLACE PROCEDURE SP_GET_KYTHI
(
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
    SELECT * FROM VW_KYTHI_ADMIN
    ORDER BY kyThiId DESC;
END;
/
CREATE OR REPLACE PROCEDURE SP_CREATE_LICHTHI
(
    p_kyThiId NUMBER,
    p_thoiGianThi TIMESTAMP,
    p_diaDiem NVARCHAR2
)
AS
BEGIN
    INSERT INTO LichThi (kyThiId, thoiGianThi, diaDiem)
    VALUES (p_kyThiId, p_thoiGianThi, p_diaDiem);
END;
/
CREATE OR REPLACE PROCEDURE SP_GET_LICHTHI_BY_KYTHI
(
    p_kyThiId NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
    SELECT *
    FROM LichThi
    WHERE kyThiId = p_kyThiId
    ORDER BY thoiGianThi;
END;
/



-- ============================================================
-- ====================        USER        ====================
-- ============================================================
-- HOME
CREATE OR REPLACE PROCEDURE SP_HOME_DASHBOARD (
    p_user_id              IN NUMBER,
    p_total_courses        OUT NUMBER,
    p_total_exams          OUT NUMBER,
    p_total_licenses       OUT NUMBER,
    p_featured_courses     OUT SYS_REFCURSOR,
    p_my_profiles          OUT SYS_REFCURSOR,
    p_my_exam_results      OUT SYS_REFCURSOR
)
AS
BEGIN
    /* ================= TỔNG QUAN CHUNG ================= */

    SELECT COUNT(*)
    INTO p_total_courses
    FROM KhoaHoc;

    SELECT COUNT(*)
    INTO p_total_exams
    FROM KyThi;

    SELECT COUNT(*)
    INTO p_total_licenses
    FROM GiayPhepLaiXe;

    /* ================= KHÓA HỌC NỔI BẬT ================= */

    OPEN p_featured_courses FOR
        SELECT
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON hg.hangId = kh.hangId
        ORDER BY kh.ngayBatDau DESC
        FETCH FIRST 6 ROWS ONLY;

    /* ================= NẾU CHƯA ĐĂNG NHẬP ================= */

    IF p_user_id IS NULL OR p_user_id = 0 THEN

        OPEN p_my_profiles FOR
            SELECT
                CAST(NULL AS NUMBER) AS hoSoId,
                CAST(NULL AS NVARCHAR2(100)) AS tenHoSo,
                CAST(NULL AS NVARCHAR2(20)) AS tenHang,
                CAST(NULL AS DATE) AS ngayDangKy,
                CAST(NULL AS NVARCHAR2(50)) AS trangThai
            FROM dual
            WHERE 1 = 0;

        OPEN p_my_exam_results FOR
            SELECT
                CAST(NULL AS NVARCHAR2(100)) AS tenBaiThi,
                CAST(NULL AS NVARCHAR2(50)) AS ketQuaDatDuoc,
                CAST(NULL AS BINARY_FLOAT) AS tongDiem,
                CAST(NULL AS NVARCHAR2(100)) AS tenKyThi
            FROM dual
            WHERE 1 = 0;

    ELSE

        /* ================= HỒ SƠ CỦA USER ================= */

        OPEN p_my_profiles FOR
            SELECT
                hs.hoSoId,
                hs.tenHoSo,
                hg.tenHang,
                hs.ngayDangKy,
                hs.trangThai
            FROM HocVien hv
            JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
            JOIN HangGplx hg ON hg.hangId = hs.hangId
            WHERE hv.userId = p_user_id
            ORDER BY hs.ngayDangKy DESC;

        /* ================= KẾT QUẢ THI CỦA USER ================= */

        OPEN p_my_exam_results FOR
            SELECT
                bt.tenBaiThi,
                ct.ketQuaDatDuoc,
                ct.tongDiem,
                kt.tenKyThi
            FROM HocVien hv
            JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
            JOIN ChiTietKetQuaThi ct ON ct.hoSoId = hs.hoSoId
            JOIN BaiThi bt ON bt.baiThiId = ct.baiThiId
            JOIN KyThi kt ON kt.kyThiId = bt.kyThiId
            WHERE hv.userId = p_user_id
            ORDER BY bt.baiThiId DESC
            FETCH FIRST 5 ROWS ONLY;

    END IF;

END;
/

-- KHÓA HỌC
CREATE OR REPLACE PROCEDURE sp_GetKhoaHocDangMo (
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.tenHang,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON kh.hangId = hg.hangId
        WHERE kh.trangThai IN (N'Sắp khai giảng', N'Đang học')
        ORDER BY kh.ngayBatDau ASC;
END;
/
CREATE OR REPLACE PROCEDURE sp_GetKhoaHocDetail (
    p_khoaHocId IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.hangId,
            hg.tenHang,
            hg.moTa,
            hg.loaiPhuongTien,
            hg.soCauHoi,
            hg.diemDat,
            hg.thoiGianTn,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON kh.hangId = hg.hangId
        WHERE kh.khoaHocId = p_khoaHocId;
END;
/
-- HỌC DASHBOASRD
CREATE OR REPLACE PROCEDURE PR_HOC_INDEX (
    P_MAHANG   IN VARCHAR2,
    P_USERID   IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG   VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH HANG_INFO AS (
            SELECT
                HG.HANGID,
                TRIM(UPPER(HG.MAHANG)) AS MAHANG,
                HG.THOIGIANTN,
                HG.SOCAUHOI
            FROM HANGGPLX HG
            WHERE TRIM(UPPER(HG.MAHANG)) = V_MAHANG
        ),
        FLAG_INFO AS (
            SELECT
                CASE
                    WHEN V_MAHANG IN ('A', 'A1') THEN 1
                    ELSE 0
                END AS IS_XEMAY
            FROM DUAL
        )
        SELECT
            H.HANGID,
            H.MAHANG,
            H.THOIGIANTN,
            (H.THOIGIANTN / 60) AS THOIGIANTHI,
            H.SOCAUHOI AS SOCAUTHINGAUNHIEN,

            (
                SELECT COUNT(*)
                FROM BODETHITHU B
                WHERE B.HANGID = H.HANGID
                  AND B.HOATDONG = 1
            ) AS TOTAL_BODE,

            CASE
                WHEN P_USERID IS NULL THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BAILAM BL
                    JOIN BODETHITHU B ON B.BODEID = BL.BODEID
                    WHERE BL.USERID = P_USERID
                      AND B.HANGID = H.HANGID
                )
            END AS DONE_BODE,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAUHOI,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE CH.CAULIET = 1
                  AND (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAULIET,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE CH.CHUY = 1
                  AND (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAUCHUY,

            (
                SELECT COUNT(*)
                FROM BIENBAO
            ) AS TOTAL_BIENBAO,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE 1
            END AS HAS_MOPHONG,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BODEMOPHONG BMP
                    WHERE BMP.ISACTIVE = 1
                )
            END AS MP_BODE,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM TINHHUONGMOPHONG
                )
            END AS MP_TINHHUONG,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') OR P_USERID IS NULL THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BAILAMMOPHONG BLM
                    JOIN BODEMOPHONG BMP ON BMP.BODEMOPHONGID = BLM.BODEMOPHONGID
                    WHERE BLM.USERID = P_USERID
                      AND BMP.ISACTIVE = 1
                )
            END AS MP_BODE_DONE
        FROM HANG_INFO H;
END;
/
CREATE OR REPLACE PROCEDURE PR_HOC_CAU_LIET (
    P_MAHANG   IN VARCHAR2,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH QUESTION_BASE AS (
            SELECT
                CH.CAUHOIID,
                CH.CHUONGID,
                CH.NOIDUNG,
                CH.HINHANH,
                CH.CAULIET,
                CH.CHUY,
                CH.XEMAY,
                CH.URLANHMEO,
                C.TENCHUONG,
                NVL(C.THUTU, 0) AS CHUONG_THUTU,
                ROW_NUMBER() OVER (
                    ORDER BY NVL(C.THUTU, 0), CH.CAUHOIID
                ) AS GLOBAL_INDEX
            FROM CAUHOILYTHUYET CH
            JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        )
        SELECT
            Q.GLOBAL_INDEX,
            Q.CAUHOIID,
            Q.NOIDUNG,
            Q.HINHANH,
            Q.URLANHMEO,
            Q.CAULIET,
            Q.CHUY,
            Q.XEMAY,
            Q.CHUONGID,
            Q.TENCHUONG,
            Q.CHUONG_THUTU,
            D.DAPANID,
            D.THUTU AS DAPAN_THUTU,
            D.DAPANDUNG
        FROM QUESTION_BASE Q
        JOIN DAPAN D ON D.CAUHOIID = Q.CAUHOIID
        WHERE Q.CAULIET = 1
          AND (
                V_MAHANG NOT IN ('A', 'A1')
                OR Q.XEMAY = 1
              )
        ORDER BY Q.CHUONG_THUTU, Q.CAUHOIID, D.THUTU;
END;
/
CREATE OR REPLACE PROCEDURE PR_HOC_CHU_Y (
    P_MAHANG   IN VARCHAR2,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH QUESTION_BASE AS (
            SELECT
                CH.CAUHOIID,
                CH.CHUONGID,
                CH.NOIDUNG,
                CH.HINHANH,
                CH.CAULIET,
                CH.CHUY,
                CH.XEMAY,
                CH.URLANHMEO,
                C.TENCHUONG,
                NVL(C.THUTU, 0) AS CHUONG_THUTU,
                ROW_NUMBER() OVER (
                    ORDER BY NVL(C.THUTU, 0), CH.CAUHOIID
                ) AS GLOBAL_INDEX
            FROM CAUHOILYTHUYET CH
            JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        )
        SELECT
            Q.GLOBAL_INDEX,
            Q.CAUHOIID,
            Q.NOIDUNG,
            Q.HINHANH,
            Q.URLANHMEO,
            Q.CAULIET,
            Q.CHUY,
            Q.XEMAY,
            Q.CHUONGID,
            Q.TENCHUONG,
            Q.CHUONG_THUTU,
            D.DAPANID,
            D.THUTU AS DAPAN_THUTU,
            D.DAPANDUNG
        FROM QUESTION_BASE Q
        JOIN DAPAN D ON D.CAUHOIID = Q.CAUHOIID
        WHERE Q.CHUY = 1
          AND (
                V_MAHANG NOT IN ('A', 'A1')
                OR Q.XEMAY = 1
              )
        ORDER BY Q.CHUONG_THUTU, Q.CAUHOIID, D.THUTU;
END;
/
CREATE OR REPLACE PROCEDURE PR_FLASHCARD_BIENBAO (
    P_USERID   IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            B.IDBIENBAO,
            B.TENBIENBAO,
            B.YNGHIA,
            B.HINHANH,
            F.IDFLASHCARD,
            F.DANHGIA
        FROM BIENBAO B
        LEFT JOIN FLASHCARD F
               ON F.IDBIENBAO = B.IDBIENBAO
              AND F.USERID = P_USERID
        ORDER BY B.IDBIENBAO;
END;
/
CREATE OR REPLACE PROCEDURE PR_FLASHCARD_SAVE (
    P_USERID      IN NUMBER,
    P_IDBIENBAO   IN NUMBER,
    P_DANHGIA     IN NVARCHAR2
)
AS
    V_COUNT NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO V_COUNT
    FROM FLASHCARD
    WHERE USERID = P_USERID
      AND IDBIENBAO = P_IDBIENBAO;

    IF V_COUNT = 0 THEN
        INSERT INTO FLASHCARD (DANHGIA, USERID, IDBIENBAO)
        VALUES (P_DANHGIA, P_USERID, P_IDBIENBAO);
    ELSE
        UPDATE FLASHCARD
        SET DANHGIA = P_DANHGIA
        WHERE USERID = P_USERID
          AND IDBIENBAO = P_IDBIENBAO;
    END IF;
END;
/
CREATE OR REPLACE PROCEDURE PR_GET_HANG
(
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT MAHANG FROM HANGGPLX ORDER BY MAHANG;
END;
/
-- LÝ THUYẾT
CREATE OR REPLACE VIEW VW_BODE_LYTHUYET AS
SELECT 
    B.BODEID,
    B.TENBODE,
    NVL(B.THOIGIAN, H.THOIGIANTN) AS THOIGIAN,
    NVL(B.SOCAUHOI, H.SOCAUHOI) AS SOCAUHOI,
    B.HANGID,
    H.MAHANG,
    H.DIEMDAT,
    H.THOIGIANTN
FROM BODETHITHU B
JOIN HANGGPLX H ON H.HANGID = B.HANGID
WHERE B.HOATDONG = 1;
/

CREATE OR REPLACE PROCEDURE PR_GET_BODE_BY_HANG
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT *
    FROM VW_BODE_LYTHUYET
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG))
    ORDER BY BODEID;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_EXAM_DETAIL
(
    P_BODEID IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        B.BODEID,
        B.TENBODE,
        NVL(B.THOIGIAN, H.THOIGIANTN) AS THOIGIAN,
        NVL(B.SOCAUHOI, H.SOCAUHOI) AS SOCAUHOI,
        H.DIEMDAT,
        H.MAHANG,
        CH.CAUHOIID,
        CH.NOIDUNG,
        CH.HINHANH,
        CH.URLANHMEO,
        NVL(CH.CAULIET, 0) AS CAULIET,
        D.DAPANID,
        D.THUTU,
        NVL(D.DAPANDUNG, 0) AS DAPANDUNG
    FROM BODETHITHU B
    JOIN HANGGPLX H ON H.HANGID = B.HANGID
    JOIN CHITIETBODETRACNGHIEM CT ON CT.BODEID = B.BODEID
    JOIN CAUHOILYTHUYET CH ON CH.CAUHOIID = CT.CAUHOIID
    JOIN DAPAN D ON D.CAUHOIID = CH.CAUHOIID
    WHERE B.BODEID = P_BODEID
      AND B.HOATDONG = 1
    ORDER BY NVL(CT.THUTU, 999999), CH.CAUHOIID, D.THUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_HISTORY_EXAM
(
    P_USERID IN NUMBER,
    P_BODEID IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        BL.BAILAMID,
        BL.THOIGIANLAMBAI,
        BL.SOCAUSAI,
        NVL(BL.KETQUA, 0) AS KETQUA,
        CT.CAUHOIID,
        CT.DAPANDACHON,
        NVL(CT.KETQUACAU, 0) AS KETQUACAU
    FROM BAILAM BL
    JOIN CHITIETBAILAM CT ON CT.BAILAMID = BL.BAILAMID
    WHERE BL.BAILAMID = (
        SELECT MAX(BL2.BAILAMID)
        FROM BAILAM BL2
        WHERE BL2.USERID = P_USERID
          AND BL2.BODEID = P_BODEID
    )
    ORDER BY CT.CAUHOIID;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_LAST_BAILAM_BY_HANG
(
    P_USERID IN NUMBER,
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        X.BODEID,
        X.BAILAMID,
        X.THOIGIANLAMBAI,
        X.SOCAUSAI,
        X.KETQUA
    FROM
    (
        SELECT
            BL.BODEID,
            BL.BAILAMID,
            BL.THOIGIANLAMBAI,
            BL.SOCAUSAI,
            NVL(BL.KETQUA, 0) AS KETQUA,
            ROW_NUMBER() OVER (PARTITION BY BL.BODEID ORDER BY BL.BAILAMID DESC) AS RN
        FROM BAILAM BL
        JOIN BODETHITHU B ON B.BODEID = BL.BODEID
        JOIN HANGGPLX H ON H.HANGID = B.HANGID
        WHERE BL.USERID = P_USERID
          AND UPPER(TRIM(H.MAHANG)) = UPPER(TRIM(P_MAHANG))
    ) X
    WHERE X.RN = 1;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_HANG_INFO
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT
        HANGID,
        MAHANG,
        SOCAUHOI,
        DIEMDAT,
        THOIGIANTN
    FROM HANGGPLX
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG));
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_ALL_QUESTION_FOR_HANG
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    V_IS_XEMAY NUMBER := 0;
BEGIN
    IF UPPER(TRIM(P_MAHANG)) IN ('A', 'A1') THEN
        V_IS_XEMAY := 1;
    END IF;

    OPEN P_CURSOR FOR
    WITH QUESTION_BASE AS
    (
        SELECT
            ROW_NUMBER() OVER (ORDER BY NVL(C.THUTU, 999999), CH.CAUHOIID) AS GLOBAL_INDEX,
            CH.CAUHOIID,
            CH.NOIDUNG,
            CH.HINHANH,
            CH.URLANHMEO,
            NVL(CH.CAULIET, 0) AS CAULIET,
            NVL(CH.CHUY, 0) AS CHUY,
            NVL(CH.XEMAY, 0) AS XEMAY,
            C.CHUONGID,
            C.TENCHUONG,
            NVL(C.THUTU, 999999) AS CHUONGTHUTU,
            D.DAPANID,
            D.THUTU AS DAPANTHUTU,
            NVL(D.DAPANDUNG, 0) AS DAPANDUNG
        FROM CAUHOILYTHUYET CH
        JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        JOIN DAPAN D ON D.CAUHOIID = CH.CAUHOIID
        WHERE (V_IS_XEMAY = 0 OR NVL(CH.XEMAY, 0) = 1)
    )
    SELECT *
    FROM QUESTION_BASE
    ORDER BY CHUONGTHUTU, CAUHOIID, DAPANTHUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_RANDOM_EXAM
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    V_HANGID NUMBER;
    V_SOCAU NUMBER;
    V_DIEMDAT NUMBER;
    V_THOIGIANTN NUMBER;
    V_IS_XEMAY NUMBER := 0;
BEGIN
    SELECT HANGID, SOCAUHOI, DIEMDAT, THOIGIANTN
    INTO V_HANGID, V_SOCAU, V_DIEMDAT, V_THOIGIANTN
    FROM HANGGPLX
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG));

    IF UPPER(TRIM(P_MAHANG)) IN ('A', 'A1') THEN
        V_IS_XEMAY := 1;
    END IF;

    OPEN P_CURSOR FOR
    WITH QBASE AS
    (
        SELECT
            CH.CAUHOIID,
            CH.NOIDUNG,
            CH.HINHANH,
            CH.URLANHMEO,
            NVL(CH.CAULIET, 0) AS CAULIET
        FROM CAUHOILYTHUYET CH
        WHERE (V_IS_XEMAY = 0 OR NVL(CH.XEMAY, 0) = 1)
    ),
    RANDQ AS
    (
        SELECT *
        FROM
        (
            SELECT Q.*, DBMS_RANDOM.VALUE AS RND
            FROM QBASE Q
        )
        ORDER BY RND
        FETCH FIRST V_SOCAU ROWS ONLY
    )
    SELECT
        -1 AS BODEID,
        N'Đề thi ngẫu nhiên hạng ' || P_MAHANG AS TENBODE,
        V_THOIGIANTN AS THOIGIAN,
        V_SOCAU AS SOCAUHOI,
        V_DIEMDAT AS DIEMDAT,
        P_MAHANG AS MAHANG,
        R.CAUHOIID,
        R.NOIDUNG,
        R.HINHANH,
        R.URLANHMEO,
        R.CAULIET,
        D.DAPANID,
        D.THUTU,
        NVL(D.DAPANDUNG, 0) AS DAPANDUNG
    FROM RANDQ R
    JOIN DAPAN D ON D.CAUHOIID = R.CAUHOIID
    ORDER BY R.CAUHOIID, D.THUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_SAVE_BAILAM
(
    P_USERID IN NUMBER,
    P_BODEID IN NUMBER,
    P_THOIGIAN IN NUMBER,
    P_SOCAUSAI IN NUMBER,
    P_KETQUA IN NUMBER,
    P_BAILAMID OUT NUMBER
)
AS
BEGIN
    INSERT INTO BAILAM(USERID, BODEID, THOIGIANLAMBAI, SOCAUSAI, KETQUA)
    VALUES (P_USERID, P_BODEID, P_THOIGIAN, P_SOCAUSAI, P_KETQUA)
    RETURNING BAILAMID INTO P_BAILAMID;
END;
/

CREATE OR REPLACE PROCEDURE PR_SAVE_CT_BAILAM
(
    P_BAILAMID IN NUMBER,
    P_CAUHOIID IN NUMBER,
    P_DAPAN IN NVARCHAR2,
    P_KETQUA IN NUMBER
)
AS
BEGIN
    INSERT INTO CHITIETBAILAM(BAILAMID, CAUHOIID, DAPANDACHON, KETQUACAU)
    VALUES (P_BAILAMID, P_CAUHOIID, P_DAPAN, P_KETQUA);
END;
/

CREATE OR REPLACE FUNCTION FUNC_CHAM_DIEM
(
    P_BAILAMID IN NUMBER
)
RETURN NUMBER
AS
    V_DUNG NUMBER := 0;
BEGIN
    SELECT COUNT(*)
    INTO V_DUNG
    FROM CHITIETBAILAM
    WHERE BAILAMID = P_BAILAMID
      AND KETQUACAU = 1;

    RETURN V_DUNG;
END;
/





COMMIT;
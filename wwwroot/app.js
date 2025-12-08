// Ajusta host según tu backend
const host = "https://localhost:44395"; 

// PRE‑LLENAR credenciales y usar endpoint de desarrollo
document.addEventListener('DOMContentLoaded', () => {
    const u = document.getElementById('username');
    const p = document.getElementById('password');
    if (u) u.value = "Bryan";
    if (p) p.value = "123";
});

const loginPath = "/api/dev/login"; // endpoint de desarrollo

const btnLogin = document.getElementById('btnLogin');
const username = document.getElementById('username');
const password = document.getElementById('password');
const loginMsg = document.getElementById('loginMsg');
const btnSpinner = document.getElementById('btnSpinner');

function showError(msg) {
    loginMsg.textContent = msg;
    loginMsg.classList.remove('text-success');
    loginMsg.classList.add('text-danger');
}

function showSuccess(msg) {
    loginMsg.textContent = msg;
    loginMsg.classList.remove('text-danger');
    loginMsg.classList.add('text-success');
}

function setLoading(on) {
    if (on) {
        btnSpinner.classList.remove('d-none');
        btnLogin.disabled = true;
    } else {
        btnSpinner.classList.add('d-none');
        btnLogin.disabled = false;
    }
}

btnLogin.addEventListener('click', async () => {
    if (!username.value.trim()) {
        username.classList.add('is-invalid');
        return;
    } else username.classList.remove('is-invalid');

    if (!password.value.trim()) {
        password.classList.add('is-invalid');
        return;
    } else password.classList.remove('is-invalid');

    setLoading(true);
    showError('');

    try {
        const res = await fetch(`${host}${loginPath}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: username.value.trim(), password: password.value })
        });

        if (!res.ok) {
            const txt = await res.text();
            showError(`Error ${res.status}: ${txt || res.statusText}`);
            setLoading(false);
            return;
        }

        const json = await res.json();
        if (json && json.accessToken) {
            localStorage.setItem('token', json.accessToken);
            showSuccess('Login correcto. Redirigiendo...');
            setTimeout(() => window.location.href = '/index.html', 700);
        } else {
            showError('Respuesta inválida del servidor');
        }
    } catch (err) {
        showError('Error de conexión: ' + err.message);
    } finally {
        setLoading(false);
    }
});

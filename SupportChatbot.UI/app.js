const apiBase = 'http://localhost:5151/api/v1.0';
const apiUrl  = 'http://localhost:5151';
let accessToken, currentUser, role, connection, currentChatId;

// DOM refs
const loginSection = document.getElementById('loginSection'),
      dashboard    = document.getElementById('dashboard'),
      chatSection  = document.getElementById('chatSection'),
      welcomeText  = document.getElementById('welcomeText'),
      loginError   = document.getElementById('loginError');

document.getElementById('loginBtn').onclick       = loginUser;
document.getElementById('startChatBtn').onclick   = startChat;
document.getElementById('sendBtn').onclick        = sendMessage;
document.getElementById('endChatBtn').onclick     = endChat;
window.joinAgentSession = joinAgentSession; // for global handler

async function loginUser() {
  const email = document.getElementById('email').value,
        password = document.getElementById('password').value;
  if (!email || !password) return loginError.textContent = 'Email/password required.';

  const resp = await fetch(`${apiBase}/auth/login`, {
    method: 'POST',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify({ email, password })
  });
  if (!resp.ok) return loginError.textContent = 'Login failed.';
  
  const data = await resp.json();
  accessToken = data.accessToken;
  saveSession(data, await fetchProfile());
  displayDashboard();
  connectSignalR();
}

function saveSession({ accessToken, refreshToken }, profile) {
  currentUser = profile;
  role = profile.role;
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('user', JSON.stringify(profile));
}

async function fetchProfile() {
  const resp = await fetch(`${apiBase}/auth/me`, {
    headers: { 'Authorization': `Bearer ${accessToken}` }
  });
  return await resp.json();
}

function displayDashboard() {
  loginSection.style.display = 'none';
  dashboard.style.display = 'block';
  welcomeText.textContent = `Welcome, ${currentUser.username || currentUser.email}`;
  document.getElementById('userUI').style.display  = role === 'User'  ? 'block' : 'none';
  document.getElementById('agentUI').style.display = role === 'Agent' ? 'block' : 'none';
}

function connectSignalR() {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${apiUrl}/chatHub`, { accessTokenFactory: () => accessToken })
    .withAutomaticReconnect()
    .build();

  connection.on('ReceiveNotification', data => {
    console.log('Notification received:', data);
    if (role === 'Agent') {
      const div = document.createElement('div');
      div.className = 'chat-card';
      div.id = `req-${data.sessionId}`;
      div.innerHTML = `<p>Session: ${data.sessionId}</p>
                       <button onclick="joinAgentSession('${data.sessionId}')">Join</button>`;
      document.getElementById('incomingRequests').appendChild(div);
    }
  });

  connection.on('ReceiveMessage', msg => {
    const isOwn = msg.senderId === currentUser.id;
    appendMessage(isOwn ? 'You' : (role==='Agent'?'User':'Agent'),
                  msg.content, isOwn ? 'own' : 'other');
  });

  connection.on('ChatEnded', info => {
    appendMessage('System', `Chat ended at ${new Date(info.endedAt).toLocaleString()}`, 'system');
    resetChatUI();
    document.getElementById(`req-${info.sessionId}`)?.remove();
  });

  connection.start().catch(console.error);
}

async function startChat() {
  const resp = await fetch(`${apiBase}/chats/start`, {
    method: 'POST',
    headers: {
      'Content-Type':'application/json',
      'Authorization': `Bearer ${accessToken}`
    },
    body: JSON.stringify({ userId: currentUser.id })
  });
  if (!resp.ok) return alert('Failed to start chat.');
  const chat = await resp.json();
  joinSession(chat.id);
}

async function joinAgentSession(sessionId) {
  joinSession(sessionId);
}

async function joinSession(chatId) {
  currentChatId = chatId;
  await connection.invoke('JoinChatGroup', chatId);
  chatSection.style.display = 'block';
  dashboard.style.display = 'none';
}

async function sendMessage() {
  const input = document.getElementById('messageInput');
  const fileInput = document.getElementById('fileInput');

  const text = input.value.trim();
  const file = fileInput.files[0];

  // Send FILE message
  if (file && connection) {
    try {
      // Step 1: Upload file
      const form = new FormData();
      form.append('ChatSessionId', currentChatId);
      form.append('UploaderId', currentUser.id);
      form.append('File', file);

      const uploadResp = await fetch(`${apiBase}/files/upload`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${accessToken}`
        },
        body: form
      });

      console.log("Upload response status:", uploadResp.status);
      if (!uploadResp.ok) {
        alert("File upload failed.");
        return;
      }

      const uploadedFile = await uploadResp.json();
      const fileMessageContent = `[file:${uploadedFile.fileName}]`;

      // Step 2: Save file message to DB
      await fetch(`${apiBase}/chats/messages`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify({
          chatSessionId: currentChatId,
          senderId: currentUser.id,
          content: fileMessageContent,
          isFile: true
        })
      });

      // Step 3: Broadcast via SignalR
      await connection.invoke('SendMessageToGroup', currentChatId, currentUser.id, fileMessageContent);

      fileInput.value = ''; // Reset file input
    } catch (err) {
      console.error("Error uploading file or sending message:", err);
      alert("An error occurred while sending the file.");
    }
  }

  // Send TEXT message
  if (text && connection) {
    try {
      // Step 1: Save text message to DB
      await fetch(`${apiBase}/chats/messages`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify({
          chatSessionId: currentChatId,
          senderId: currentUser.id,
          content: text,
          isFile: false
        })
      });

      // Step 2: Broadcast via SignalR
      await connection.invoke('SendMessageToGroup', currentChatId, currentUser.id, text);

      input.value = ''; // Clear input field
    } catch (err) {
      console.error("Error sending message:", err);
      alert("An error occurred while sending the message.");
    }
  }
}


async function endChat() {
  await connection.invoke('EndChat', currentChatId);
}

function appendMessage(sender, content, type) {
  const box = document.getElementById('chatBox'),
        div = document.createElement('div');
  div.className = `message ${type}`;
  div.innerHTML = `<strong>${sender}:</strong><br/>${formatMessage(content)}`;
  box.appendChild(div);
  box.scrollTop = box.scrollHeight;
}

function formatMessage(content) {
  const m = content.match(/\[file:(.+?)\]/);
  if (m) {
    const fn = m[1];
    const ext = fn.split('.').pop().toLowerCase(),
          url = `${apiUrl}/files/${fn}`;
    return ['jpg','png','gif','jpeg'].includes(ext)
      ? `<img src="${url}" class="chat-image" />`
      : `<a href="${url}" target="_blank">Download ${fn}</a>`;
  }
  return content;
}

function resetChatUI() {
  chatSection.style.display = 'none';
  dashboard.style.display = 'block';
  currentChatId = null;
}

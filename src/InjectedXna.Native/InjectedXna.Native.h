// InjectedXna.Native.h

#pragma once

#include <d3d9.h>

namespace InjectedXna {
	public ref class StateBlock
	{
	private:
		IDirect3DDevice9* m_pDevice;
		IDirect3DStateBlock9* m_pStateBlock;

	public:
		StateBlock(System::IntPtr^ pDevice) {
			if (pDevice == System::IntPtr::Zero) {
				throw gcnew System::ArgumentNullException("pDevice");
			}

			m_pDevice = (IDirect3DDevice9*)pDevice->ToPointer();
			pin_ptr<IDirect3DStateBlock9*> pin_pStateBlock = &m_pStateBlock;
			m_pDevice->CreateStateBlock(D3DSBT_ALL, pin_pStateBlock);
		}

		~StateBlock() {
			if (m_pStateBlock) {
				m_pStateBlock->Release();
				m_pStateBlock = NULL;
			}
		}

		System::Int32 Capture() {
			if (!m_pStateBlock) {
				throw gcnew System::InvalidOperationException("State block has not been crated correctly");
			}
			return (System::Int32)m_pStateBlock->Capture();
		}

		System::Int32 Apply() {
			if (!m_pStateBlock) {
				throw gcnew System::InvalidOperationException("State block has not been crated correctly");
			}
			return (System::Int32)m_pStateBlock->Apply();
		}
	};
}
#if !defined(MAUTO_PTR)

#ifndef __cplusplus_cli
#error ERROR: msclr libraries are not compatible with /clr:oldSyntax
#endif

#include <msclr\safebool.h>
#include "HttpSysManager.h"


using namespace msclr;

BeginHttpSysManager

    // wrap a resource to enforce strict ownership and ensure proper cleanup
    template<typename _element_type>
    public ref class mauto_handle
    {
    private:
        // disallow explicit comparisons to _safe_bool
        bool operator==( _detail_class::_safe_bool );
        bool operator!=( _detail_class::_safe_bool );
        
    public:

        // Constructors
        
        mauto_handle() 
            : m_handle( nullptr )
        {
        }

        // construct from object pointer
        mauto_handle( _element_type * _ptr ) 
            : m_handle( _ptr )
        {
        }

        // construct by assuming pointer from _right auto_handle
        mauto_handle(mauto_handle<_element_type> % _right ) 
                : m_handle( _right.release() )
        {
        }

        template<typename _other_type>
        mauto_handle( mauto_handle<_other_type> % _right ) 
                : m_handle( _right.release() )
        {
        }

        // assign compatible _right
        mauto_handle<_element_type> % operator=( 
            mauto_handle<_element_type> % _right ) 
        {
            reset( _right.release() );
            return *this;
        }


        template<typename _other_type>
        mauto_handle<_element_type> % operator=(
            mauto_handle<_other_type> % _right ) 
        {
            reset( _right.release() );
            return *this;
        }

        _element_type * get()
        {
            return m_handle;
        }

        // return pointer to class object (assume pointer)
        _element_type * operator->()
        {
            return m_handle;
        }

        // for use when auto_handle appears in a conditional
        operator _detail_class::_safe_bool()
        {
            return valid() ? _detail_class::_safe_true : _detail_class::_safe_false;
        }

        // for use when auto_handle appears in a conditional
        bool operator!()
        {
            return ! valid();
        }


        template<typename _other_type>
        operator mauto_handle<_other_type>() 
        {
            return auto_handle<_other_type>( *this );
        }

        void swap( mauto_handle<_element_type> % _right ) 
        {
			mauto_handle<_element_type> tmp = _right;
			_right = *this;
			*this = tmp;
        }

        void reset( _element_type * _new_ptr ) 
        {
            if( m_handle != _new_ptr )
            {
                if( valid() )
                {
                    delete m_handle;
                }
                m_handle = _new_ptr;
            }
        }

        void reset( ) 
        {
            reset(nullptr);
        }

        _element_type * release() 
        {
            _element_type *_tmp_ptr = m_handle;
            m_handle = nullptr;
            return _tmp_ptr;
        }

        // destroy the object
        ~mauto_handle()
        {
            (*this).!mauto_handle();
        }
		 // destroy the object
        !mauto_handle()
        {
            if( valid() )
            {
                delete m_handle;
            }
        }

        private:

        bool valid()
        {
            // see if the managed resource is in the invalid state.
            return m_handle != nullptr;

        }

        // the wrapped object
        _element_type * m_handle;
    };

    // swap the contents of two auto_handle objects
    template<typename _element_type>
    void swap( mauto_handle<_element_type> % _left, 
               mauto_handle<_element_type> % _right ) 
    {
        _left.swap( _right );
    }

EndHttpSysManager

#define MAUTO_PTR

#endif // MAUTO_PTR
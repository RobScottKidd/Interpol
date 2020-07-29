/*
MIT License
Copyright (c) 2019 Lenny Kean
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.ServiceModel.Channels;

namespace WcfCoreMtomEncoder
{
    public class MtomMessageEncoderBindingElement : MessageEncodingBindingElement
    {
        private readonly MessageEncodingBindingElement _innerEncodingBindingElement;
        
        public MtomMessageEncoderBindingElement(MessageEncodingBindingElement innerEncodingBindingElement)
        {
            _innerEncodingBindingElement = innerEncodingBindingElement;
        }

        public override BindingElement Clone()
        {
            return new MtomMessageEncoderBindingElement(_innerEncodingBindingElement);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new MtomMessageEncoderFactory(_innerEncodingBindingElement.CreateMessageEncoderFactory());
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.BindingParameters.Add(this);

            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return _innerEncodingBindingElement.CanBuildChannelFactory<TChannel>(context);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return _innerEncodingBindingElement.GetProperty<T>(context);
        }

        public override MessageVersion MessageVersion
        {
            get => _innerEncodingBindingElement.MessageVersion;
            set => _innerEncodingBindingElement.MessageVersion = value;
        }
    }
}